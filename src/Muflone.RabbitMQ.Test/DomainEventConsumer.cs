using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Muflone.Messages.Events;
using Muflone.RabbitMQ.Consumers;
using RabbitMQ.Client;

namespace Muflone.RabbitMQ.Test
{
    public class DomainEventConsumer<TEvent> : DomainEventConsumerBase<TEvent> where TEvent : DomainEvent
    {
        public DomainEventConsumer(IBusControl busControl,
            IDomainEventHandler<TEvent> eventHandler,
            ILoggerFactory loggerFactory) : base(busControl, eventHandler, loggerFactory)
        {
        }

        public override Task Consume(CancellationToken cancellationToken = default)
        {
            this.BusControl.RabbitMQChannel.BasicConsume(typeof(TEvent).Name, true, this.RabbitMQConsumer);

            return Task.CompletedTask;
        }
    }
}