using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Muflone.Messages;
using Muflone.Messages.Commands;
using Muflone.Messages.Events;
using Muflone.RabbitMQ.Abstracts;
using Muflone.RabbitMQ.Factories;
using Muflone.RabbitMQ.Helpers;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Muflone.RabbitMQ
{
    public sealed class BusControl : IBusControl
    {
        private readonly BrokerProperties brokerProperties;

        private AsyncEventingBasicConsumer rabbitMQConsumer;
        private readonly ILogger logger;

        private readonly ISubscriberRegistry subscriberRegistry;
        private readonly IServiceProvider serviceProvider;

        private readonly MessageHandlerFactory messageHandlerFactory;
        private readonly CommandHandlerFactory commandHandlerFactory;
        private readonly DomainEventHandlerFactory domainEventHandlerFactory;

        public IModel RabbitMQChannel { get; private set; }

        public BusControl(ISubscriberRegistry subscriberRegistry,
            IServiceProvider serviceProvider,
            IOptions<BrokerProperties> options,
            ILoggerFactory loggerFactory)
        {
            if (subscriberRegistry == null)
                throw new NullReferenceException(nameof(subscriberRegistry));

            if (subscriberRegistry.Observers == null || !subscriberRegistry.Observers.Any())
                throw new Exception("No handlers found! At least one handler for message is required!");

            this.subscriberRegistry = subscriberRegistry;
            this.serviceProvider = serviceProvider;
            this.brokerProperties = options.Value;

            messageHandlerFactory = new MessageHandlerFactory(serviceProvider);
            commandHandlerFactory = new CommandHandlerFactory(serviceProvider);
            domainEventHandlerFactory = new DomainEventHandlerFactory(serviceProvider);

            this.logger = loggerFactory.CreateLogger(GetType());
        }

        public Task Start(CancellationToken cancellationToken = default)
        {
            if (cancellationToken.IsCancellationRequested)
                cancellationToken.ThrowIfCancellationRequested();

            var connectionFactory = RabbitMqFactories.CreateConnectionFactory(brokerProperties);
            var connection = RabbitMqFactories.CreateConnection(connectionFactory);

            RabbitMQChannel = RabbitMqFactories.CreateChannel(connection);

            return Task.CompletedTask;
        }

        public Task Stop(CancellationToken cancellationToken = default)
        {
            if (cancellationToken.IsCancellationRequested)
                cancellationToken.ThrowIfCancellationRequested();

            if (!RabbitMQChannel.IsClosed)
                RabbitMQChannel.Close();

            RabbitMQChannel.Dispose();

            return Task.CompletedTask;
        }

        public Task RegisterConsumer<T>(T message, CancellationToken cancellationToken = default) where T : class, IMessage
        {
            if (cancellationToken.IsCancellationRequested)
                cancellationToken.ThrowIfCancellationRequested();

            RabbitMQChannel.QueueDeclare(typeof(T).Name, true, false, false, null);

            rabbitMQConsumer = RabbitMqFactories.CreateAsyncEventingBasicConsumer(RabbitMQChannel);
            rabbitMQConsumer.Received += MessageConsumer<T>;

            RabbitMQChannel.BasicConsume(typeof(T).Name, true, rabbitMQConsumer);

            return Task.CompletedTask;
        }

        private Task MessageConsumer<T>(object sender, BasicDeliverEventArgs @event) where T : class, IMessage
        {
            try
            {
                var message = RabbitMqMappers.MapRabbitMqMessageToMuflone<T>(@event.Body.ToArray());

                var messageHandlers = GetMessageHandlers<T>();

                foreach (var handler in messageHandlers)
                {
                    handler.Handle(message);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(
                    $"Message {typeof(T).Name} - Error: {ex.Message} - StackTrace: {ex.StackTrace} - Source: {ex.Source}");
                //non vogliamo che si blocchi il dispatch
                //throw;
            }

            return Task.CompletedTask;
        }

        public Task RegisterCommandConsumer<T>(CancellationToken cancellationToken = default)
            where T : class, ICommand
        {
            if (cancellationToken.IsCancellationRequested)
                cancellationToken.ThrowIfCancellationRequested();

            RabbitMQChannel.QueueDeclare(typeof(T).Name, true, false, false, null);

            rabbitMQConsumer = RabbitMqFactories.CreateAsyncEventingBasicConsumer(RabbitMQChannel);
            rabbitMQConsumer.Received += this.CommandConsumer<T>;

            RabbitMQChannel.BasicConsume(typeof(T).Name, true, rabbitMQConsumer);

            return Task.CompletedTask;
        }

        private Task CommandConsumer<T>(object sender, BasicDeliverEventArgs @event) where T : class, ICommand
        {
            try
            {
                var message = RabbitMqMappers.MapRabbitMqMessageToMuflone<T>(@event.Body.ToArray());

                var commandHandlers = GetCommandHandlers<T>();

                foreach (var handler in commandHandlers)
                {
                    handler.Handle(message);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(
                    $"Message {typeof(T).Name} - Error: {ex.Message} - StackTrace: {ex.StackTrace} - Source: {ex.Source}");
                //non vogliamo che si blocchi il dispatch
                //throw;
            }

            return Task.CompletedTask;
        }

        public Task RegisterEventConsumer<T>(CancellationToken cancellationToken = default(CancellationToken)) where T : class, IEvent
        {
            if (cancellationToken.IsCancellationRequested)
                cancellationToken.ThrowIfCancellationRequested();

            RabbitMQChannel.QueueDeclare(typeof(T).Name, true, false, false, null);

            rabbitMQConsumer = RabbitMqFactories.CreateAsyncEventingBasicConsumer(RabbitMQChannel);
            rabbitMQConsumer.Received += EventConsumer<T>;

            RabbitMQChannel.BasicConsume(typeof(T).Name, true, rabbitMQConsumer);

            return Task.CompletedTask;
        }

        private Task EventConsumer<T>(object sender, BasicDeliverEventArgs @event) where T : class, IEvent
        {
            try
            {
                var message = RabbitMqMappers.MapRabbitMqMessageToMuflone<T>(@event.Body.ToArray());

                if (typeof(T) is IDomainEvent)
                {
                    //var domainEventHandler = GetDomainEventHandlers<T>();
                }

                //foreach (var handler in domainEventHandler)
                //{
                //    handler.Handle(message);
                //}
            }
            catch (Exception ex)
            {
                logger.LogError(
                    $"Message {typeof(T).Name} - Error: {ex.Message} - StackTrace: {ex.StackTrace} - Source: {ex.Source}");
                //non vogliamo che si blocchi il dispatch
                //throw;
            }

            return Task.CompletedTask;
        }

        private IEnumerable<IMessageHandler<T>> GetMessageHandlers<T>() where T : class, IMessage
        {
            return new List<IMessageHandler<T>>(
                subscriberRegistry.Get<T>()
                    .Select(handlerType => this.messageHandlerFactory.GetMessageHandler<T>())
            );
        }

        private IEnumerable<CommandHandler<T>> GetCommandHandlers<T>() where T : class, ICommand
        {
            return new List<CommandHandler<T>>(
                subscriberRegistry.Get<T>()
                    .Select(handlerType => commandHandlerFactory.GetCommandHandler<T>())
                    .Cast<CommandHandler<T>>()
                );
        }

        private IEnumerable<DomainEventHandler<T>> GetDomainEventHandlers<T>() where T : class, IDomainEvent
        {
            return new List<DomainEventHandler<T>>(
                subscriberRegistry.Get<T>()
                    .Select(handlerType => this.domainEventHandlerFactory.GetDomainEventHandler<T>())
                    .Cast<DomainEventHandler<T>>()
            );
        }
    }
}