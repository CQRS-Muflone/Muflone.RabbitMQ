using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Muflone.Messages.Events;
using Muflone.RabbitMQ.Helpers;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Muflone.RabbitMQ
{
    public class RabbitMqDomainEventConsumer<TEvent> : IDomainEventConsumer<TEvent> where TEvent: class, IDomainEvent
    {
        private readonly IDomainEventHandler<TEvent> eventHandler;
        private readonly ILogger logger;
        private readonly IModel rabbitMqChannel;

        public RabbitMqDomainEventConsumer(IDomainEventHandler<TEvent> eventHandler,
            ILoggerFactory loggerFactory, BrokerProperties brokerProperties)
        {
            this.logger = loggerFactory.CreateLogger(this.GetType());

            var connectionFactory = RabbitMqFactories.CreateConnectionFactory(brokerProperties);
            var connection = RabbitMqFactories.CreateConnection(connectionFactory);
            this.rabbitMqChannel = RabbitMqFactories.CreateChannel(connection);
            this.rabbitMqChannel.ExchangeDeclare(typeof(TEvent).Name, ExchangeType.Fanout);

            if (eventHandler == null)
                return;

            this.eventHandler = eventHandler;

            var queueName = this.rabbitMqChannel.QueueDeclare().QueueName;
            this.rabbitMqChannel.QueueBind(queueName,
                typeof(TEvent).Name,"");
            var rabbitMqConsumer = RabbitMqFactories.CreateEventingBasicCosumer(this.rabbitMqChannel);
            rabbitMqConsumer.Received += this.EventConsumer;
            this.rabbitMqChannel.BasicConsume(typeof(TEvent).Name, true, rabbitMqConsumer);
        }

        private void EventConsumer(object sender, BasicDeliverEventArgs e)
        {
            try
            {
                var mufloneEvent = RabbitMqMappers.MapRabbitMqMessageToMuflone<TEvent>(e.Body);
                this.eventHandler.Handle(mufloneEvent);
            }
            catch (Exception ex)
            {
                this.logger.LogInformation($"Original message: {e.Body}");
                this.logger.LogError($"StackTrace: {ex.StackTrace}, Source: {ex.Source}");
            }
        }

        public async Task Publish(TEvent @event, CancellationToken cancellationToken = default)
        {
            try
            {
                var messageBody = RabbitMqMappers.MapMufloneMessageToRabbitMq(@event);
                this.rabbitMqChannel.BasicPublish(typeof(TEvent).Name, "", null, messageBody);

                await Task.Yield();
            }
            catch (Exception ex)
            {
                this.logger.LogError($"StackTrace: {ex.StackTrace}, Source: {ex.Source}");
                throw new Exception($"StackTrace: {ex.StackTrace}, Source: {ex.Source}");
            }
        }
    }
}