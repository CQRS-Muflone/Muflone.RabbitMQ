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

        private readonly ILogger logger;

        private readonly ISubscriberRegistry subscriberRegistry;

        private readonly IMessageHandlerFactory messageHandlerFactory;
        private readonly ICommandHandlerFactory commandHandlerFactory;
        private readonly IDomainEventHandlerFactory domainEventHandlerFactory;
        private readonly IIntegrationEventHandlerFactory integrationEventHandlerFactory;

        private readonly IRepositoryFactory repositoryFactory;

        public IModel RabbitMQChannel { get; private set; }
        private AsyncEventingBasicConsumer rabbitMQConsumer;

        public BusControl(ISubscriberRegistry subscriberRegistry,
            IServiceProvider provider,
            IOptions<BrokerProperties> options,
            ILoggerFactory loggerFactory)
        {
            if (subscriberRegistry == null)
                throw new NullReferenceException(nameof(subscriberRegistry));

            //if (subscriberRegistry.CommandObservers == null || !subscriberRegistry.CommandObservers.Any())
            //    throw new Exception("No commandHandlers found! At least one handler for message is required!");

            this.subscriberRegistry = subscriberRegistry;
            this.brokerProperties = options.Value;

            //this.messageHandlerFactory = messageHandlerFactory;
            this.commandHandlerFactory = new CommandHandlerFactory(provider);
            this.domainEventHandlerFactory = new DomainEventHandlerFactory(provider);
            this.integrationEventHandlerFactory = new IntegrationEventHandlerFactory(provider);
            this.repositoryFactory = new RepositoryFactory(provider);

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

        public Task RegisterCommandConsumers(Type message, CancellationToken cancellationToken = default)
        {
            if (cancellationToken.IsCancellationRequested)
                cancellationToken.ThrowIfCancellationRequested();

            RabbitMQChannel.QueueDeclare(message.Name, true, false, false, null);

            rabbitMQConsumer = RabbitMqFactories.CreateAsyncEventingBasicConsumer(RabbitMQChannel);
            rabbitMQConsumer.Received += (sender, @event) =>
            {
                // Deserialize RMQ message to Muflone Command
                var command = (Command)RabbitMqMappers.MapRabbitMqMessageToMuflone(message, @event.Body);

                // Looking for handlers in CommandObservers registry
                var handlers = this.subscriberRegistry.CommandObservers[message];
                foreach (var handlerType in handlers)
                {
                    var templateHandlerType = typeof(CommandHandler<>);
                    //var x = (IStore)Activator.CreateInstance(constructedType, new object[] { someParameter });
                    var commandHandler =
                        Activator.CreateInstance(templateHandlerType, repositoryFactory.GetRepository(), logger);

                    var obj = Activator.CreateInstance(
                        Type.GetType($"{handlerType.FullName}, {handlerType.Assembly.FullName}", true, false));

                    var parameters = new object[] {this.repositoryFactory.GetRepository(), this.logger};
                    //var handlerInstance = Activator.CreateInstance(handlerType.Assembly.FullName, handlerType.FullName, false,
                    //    BindingFlags.Default, (Binder) null, parameters, CultureInfo.InvariantCulture, new object[] {});
                }

                return Task.CompletedTask;
            };

            RabbitMQChannel.BasicConsume(message.Name, true, this.rabbitMQConsumer);

            return Task.CompletedTask;
        }

        public Task RegisterConsumer<T>(CancellationToken cancellationToken = default) where T : class, IMessage
        {
            if (cancellationToken.IsCancellationRequested)
                cancellationToken.ThrowIfCancellationRequested();

            RabbitMQChannel.QueueDeclare(typeof(T).Name, true, false, false, null);

            rabbitMQConsumer = RabbitMqFactories.CreateAsyncEventingBasicConsumer(RabbitMQChannel);
            rabbitMQConsumer.Received += MessageConsumer<T>;

            RabbitMQChannel.BasicConsume(typeof(T).Name, true, this.rabbitMQConsumer);

            return Task.CompletedTask;
        }

        private Task MessageConsumer<T>(object sender, BasicDeliverEventArgs @event) where T : class, IMessage
        {
            try
            {
                var message = RabbitMqMappers.MapRabbitMqMessageToMuflone<T>(@event.Body.ToArray());

                var messageHandlers = GetMessageHandlers<T>();

                //return new List<CommandHandler<T>>(
                //    subscriberRegistry.Get<T>()
                //        .Select(handlerType => commandHandlerFactory.GetCommandHandler<T>())
                //        .Cast<CommandHandler<T>>()
                //);

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

        #region Commands
        public Task RegisterCommandConsumer<T>(CancellationToken cancellationToken = default)
            where T : class, ICommand
        {
            if (cancellationToken.IsCancellationRequested)
                cancellationToken.ThrowIfCancellationRequested();

            RabbitMQChannel.QueueDeclare(typeof(T).Name, true, false, false, null);

            this.rabbitMQConsumer = RabbitMqFactories.CreateAsyncEventingBasicConsumer(RabbitMQChannel);
            this.rabbitMQConsumer.Received += this.CommandConsumer<T>;

            RabbitMQChannel.BasicConsume(typeof(T).Name, true, this.rabbitMQConsumer);

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
        #endregion

        #region DomainEvents
        public Task RegisterEventConsumer<T>(CancellationToken cancellationToken = default(CancellationToken)) where T : class, IEvent
        {
            if (cancellationToken.IsCancellationRequested)
                cancellationToken.ThrowIfCancellationRequested();

            RabbitMQChannel.QueueDeclare(typeof(T).Name, true, false, false, null);

            this.rabbitMQConsumer = RabbitMqFactories.CreateAsyncEventingBasicConsumer(RabbitMQChannel);
            this.rabbitMQConsumer.Received += EventConsumer<T>;

            RabbitMQChannel.BasicConsume(typeof(T).Name, true, this.rabbitMQConsumer);

            return Task.CompletedTask;
        }

        private Task EventConsumer<T>(object sender, BasicDeliverEventArgs @event) where T : class, IEvent
        {
            try
            {
                var message = RabbitMqMappers.MapRabbitMqMessageToMuflone<T>(@event.Body.ToArray());

                if (message.Version == 1)
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
        #endregion

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
                subscriberRegistry.GetCommands<T>()
                    .Select(handlerType => commandHandlerFactory.GetCommandHandler<T>())
                    .Cast<CommandHandler<T>>()
                );
        }

        private IEnumerable<DomainEventHandler<T>> GetDomainEventHandlers<T>() where T : class, IDomainEvent
        {
            return new List<DomainEventHandler<T>>(
                subscriberRegistry.GetDomainEvents<T>()
                    .Select(handlerType => this.domainEventHandlerFactory.GetDomainEventHandler<T>())
                    .Cast<DomainEventHandler<T>>()
            );
        }
    }
}