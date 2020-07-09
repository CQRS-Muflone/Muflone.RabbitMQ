using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Muflone.Messages;
using Muflone.Messages.Commands;
using Muflone.RabbitMQ.Abstracts;
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
        private readonly IMessageHandlerFactory messageHandlerFactory;

        public IModel RabbitMQChannel { get; private set; }

        public BusControl(ISubscriberRegistry subscriberRegistry,
            IMessageHandlerFactory messageHandlerFactory,
            IOptions<BrokerProperties> options,
            ILoggerFactory loggerFactory)
        {
            if (subscriberRegistry == null)
                throw new NullReferenceException(nameof(subscriberRegistry));

            if (subscriberRegistry.Observers == null || !subscriberRegistry.Observers.Any())
                throw new Exception("No handlers found! At least one handler for message is required!");

            this.subscriberRegistry = subscriberRegistry;
            this.messageHandlerFactory = messageHandlerFactory;
            this.brokerProperties = options.Value;
            this.logger = loggerFactory.CreateLogger( GetType());
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

        public Task RegisterMessageConsumers( CancellationToken cancellationToken = default(CancellationToken))
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

            foreach (var observer in subscriberRegistry.Observers)
            {
                try
                {
                    // Deserialize message from RMQ to Muflone
                    var mufloneMessage = (IMessage)JsonConvert.DeserializeObject(messageBody, observer.Key);
                    if (mufloneMessage == null)
                        continue;

                    logger.LogDebug($"BusControl-Dispatch Event {mufloneMessage.GetType()}");

                    foreach (var handlerType in observer.Value)
                    {
                        var memberInfo = mufloneMessage.GetType().BaseType;
                        if (memberInfo != null && memberInfo.Name.Equals("Command"))
                        {
                            var messageHandler = this.messageHandlerFactory.GetMessageHandler(handlerType);
                            
                        }
                        //((ICommandHandler<T>)handlerType).Handle((T)mufloneMessage);


                            //var memberInfo = mufloneMessage.GetType().BaseType;
                            //if (memberInfo != null && memberInfo.Name.Equals("Command"))
                            //{
                            //    //var commandHandler = CreateCommandHandler<>(handlerType);
                            //}
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