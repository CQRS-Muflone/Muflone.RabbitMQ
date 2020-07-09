using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Muflone.Core;
using Muflone.Messages.Events;
using Muflone.Persistence;
using Xunit;

namespace Muflone.RabbitMQ.Test
{
    public class PublishDomainEventWithRabbitMqTest
    {
        private readonly IBusControl busControl;

        public PublishDomainEventWithRabbitMqTest()
        {
            var options = Options.Create(new BrokerProperties
            {
                HostName = "localhost",
                Username = "guest",
                Password = "guest",
                DispatchConsumersAsync = true
            });

            //this.busControl = new BusControl(options, new NullLoggerFactory());
        }

        [Fact]
        public async Task Can_Publish_DomainEvent_With_ServiceBus()
        {
            var options = Options.Create(new BrokerProperties
            {
                HostName = "localhost",
                Username = "guest",
                Password = "guest"
            });
            var serviceBus = new ServiceBus(this.busControl, new NullLoggerFactory());

            var myEvent = new MyEvent(new MyDomainId(Guid.NewGuid()));

            var cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = cancellationTokenSource.Token;

            await serviceBus.StartAsync(cancellationToken);
            await serviceBus.Publish(myEvent);
        }

        [Fact]
        public async Task Can_Receive_DomainEvent_With_RabbitMQ_Muflone_Provider()
        {
            var myEventHandler = new MyEventHandler(new InMemoryPersister(), new NullLoggerFactory());

            var cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = cancellationTokenSource.Token;
            //await this.busControl.RegisterConsumer(domainEventConsumer, cancellationToken);

            //this.busControl.RegisterHandler(typeof(MyEvent), myEventHandler, cancellationToken);

            Thread.Sleep(2000);
            Assert.Equal("I am a DomainEvent", TestResult.DomainEventContent);
        }

        private class MyDomainId : DomainId
        {
            public MyDomainId(Guid value) : base(value)
            {
            }
        }

        private class MyEvent: DomainEvent
        {
            public readonly string EventContent;

            public MyEvent(MyDomainId aggregateId, string eventContent = "I am a DomainEvent", string who = "anonymous") : base(aggregateId, who)
            {
                this.EventContent = eventContent;
            }
        }

        private class MyEventHandler : DomainEventHandler<MyEvent>
        {
            public MyEventHandler(IPersister persister, ILoggerFactory loggerFactory) : base(persister, loggerFactory)
            {
            }

            public override Task Handle(MyEvent @event)
            {
                TestResult.DomainEventContent = @event.EventContent;
                return Task.CompletedTask;
            }
        }

        public static class TestResult
        {
            public static string DomainEventContent;
        }
    }
}