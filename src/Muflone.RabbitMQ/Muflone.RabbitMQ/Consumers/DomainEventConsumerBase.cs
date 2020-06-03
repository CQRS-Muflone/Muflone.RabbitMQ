using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Muflone.Messages.Events;
using Muflone.RabbitMQ.Abstracts.Events;
using Muflone.RabbitMQ.Helpers;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Muflone.RabbitMQ.Consumers
{
    public class DomainEventConsumerBase<TEvent> : IDomainEventConsumer<TEvent> where TEvent: class, IDomainEvent
    {
        private readonly IBusControl busControl;
        private readonly IDomainEventHandler<TEvent> eventHandler;
        private readonly ILogger logger;

        public DomainEventConsumerBase(IBusControl busControl,
            IDomainEventHandler<TEvent> eventHandler,
            ILoggerFactory loggerFactory)
        {
            this.busControl = busControl ?? throw new NullReferenceException($"Value cannot be null. (Parameter '{nameof(busControl)}')");
            this.eventHandler = eventHandler ?? throw new NullReferenceException($"Value cannot be null. (Parameter '{nameof(eventHandler)}')");

            this.logger = loggerFactory.CreateLogger(this.GetType());

            this.eventHandler = eventHandler;

            this.busControl.RabbitMQChannel.QueueDeclare(typeof(TEvent).Name, true, false, false, null);
            this.busControl.RabbitMQChannel.QueueBind(typeof(TEvent).Name,
                typeof(TEvent).Name,"");
            var rabbitMqConsumer = RabbitMqFactories.CreateEventingBasicCosumer(this.busControl.RabbitMQChannel);
            rabbitMqConsumer.Received += this.EventConsumer;
            this.busControl.RabbitMQChannel.BasicConsume(typeof(TEvent).Name, true, rabbitMqConsumer);
        }

        private async Task EventConsumer(object sender, BasicDeliverEventArgs e)
        {
            try
            {
                var mufloneEvent = RabbitMqMappers.MapRabbitMqMessageToMuflone<TEvent>(e.Body);
                using (var handler = this.eventHandler)
                    await handler.Handle(mufloneEvent);
            }
            catch (Exception ex)
            {
                this.logger.LogInformation($"Original message: {e.Body}");
                this.logger.LogError($"StackTrace: {ex.StackTrace}, Source: {ex.Source}");
            }
        }
    }
}