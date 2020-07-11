using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Muflone.Messages;
using Muflone.Messages.Commands;
using Muflone.RabbitMQ.Abstracts;
using Muflone.RabbitMQ.Factories;
using Muflone.RabbitMQ.Helpers;
using Newtonsoft.Json;
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

        public Task RegisterMessageConsumers(CancellationToken cancellationToken = default(CancellationToken))
        {
            foreach (var observer in this.subscriberRegistry.Observers)
            {
                if (cancellationToken.IsCancellationRequested)
                    cancellationToken.ThrowIfCancellationRequested();

                RabbitMQChannel.QueueDeclare(observer.Key.Name, true, false, false, null);

                rabbitMQConsumer = RabbitMqFactories.CreateAsyncEventingBasicConsumer(RabbitMQChannel);
                rabbitMQConsumer.Received += this.MessageConsumer;

                RabbitMQChannel.BasicConsume(observer.Key.Name, true, rabbitMQConsumer);
            }

            return Task.CompletedTask;
        }

        public Task MessageConsumer(object sender, BasicDeliverEventArgs @event)
        {
            var messageBody = Encoding.UTF8.GetString(@event.Body.ToArray());

            var commandHandlerFactory = new CommandHandlerFactory(this.serviceProvider);

            foreach (var observer in subscriberRegistry.Observers)
            {
                try
                {
                    // Deserialize message from RMQ to Muflone generic IMessage
                    // We need this to discover BaseType of the current message
                    var mufloneMessage = (IMessage)JsonConvert.DeserializeObject(messageBody, observer.Key);
                    if (mufloneMessage == null)
                        continue;

                    logger.LogDebug($"BusControl-Dispatch Event {mufloneMessage.GetType()}");

                    var handler =
                        this.subscriberRegistry.Handlers.FirstOrDefault(h => h.Key.GetType().Name == observer.Key.Name);

                    var memberInfo = mufloneMessage.GetType().BaseType;
                    var handlers = serviceProvider.GetServices(observer.Value.First());

                    if (memberInfo != null && memberInfo.Name.Equals("Command"))
                    {
                        var command = (Command)JsonConvert.DeserializeObject(messageBody, observer.Key);
                        var commandHandlerType = typeof(ICommandHandler<>);

                        var specificCommandHandlerType = commandHandlerType.MakeGenericType(observer.Key);
                        var v2 = specificCommandHandlerType.GetProperty("Value")?.GetValue(observer.Key, null);

                        //foreach (var handler in handlers)
                        //{
                        //    //var customCommandHandlerType = commandHandlerType.MakeGenericType(handler.GetType());

                        //    //handler.Handle(command);
                        //}
                    }

                    if (memberInfo != null && memberInfo.Name.Equals("DomainEvent"))
                    {
                    }

                    if (memberInfo != null && memberInfo.Name.Equals("IntegrationEvent"))
                    {
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(
                        $"RMQ message {messageBody} - Error: {ex.Message} - StackTrace: {ex.StackTrace} - Source: {ex.Source}");
                    //non vogliamo che si blocchi il dispatch
                    //throw;
                }
            }

            return Task.CompletedTask;
        }

        private IMessageHandler<T> CreateHandler<T>(Type commandType) where T : IMessage
        {
            return (IMessageHandler<T>) Activator.CreateInstance(commandType);
        }

        private ICommandHandler<T> CreateCommandHandler<T>(Type handlerType) where T : ICommand
        {
            return (ICommandHandler<T>) Activator.CreateInstance(handlerType);
        }
    }
}