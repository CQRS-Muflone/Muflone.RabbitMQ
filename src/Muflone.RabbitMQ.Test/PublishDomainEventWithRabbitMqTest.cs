using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Muflone.Core;
using Muflone.Messages.Events;
using Muflone.Persistence;
using Xunit;

namespace Muflone.RabbitMQ.Test
{
    public class PublishDomainEventWithRabbitMqTest
    {
        [Fact]
        public void Cannot_Create_DomainEventConsumer_Without_BrokerProperties()
        {
            var myEventHandler = new MyEventHandler(new InMemoryPersister(), new NullLoggerFactory());

            var exception =
                Assert.ThrowsAny<Exception>(() =>
                    new DomainEventConsumerBase<MyEvent>(myEventHandler, new NullLoggerFactory(), null));

            Assert.Equal("Value cannot be null. (Parameter 'brokerProperties')", exception.Message);
        }

        [Fact]
        public async Task Can_Publish_DomainEvent_With_RabbitMQ_Muflone_Provider()
        {
            var brokerProperties = new BrokerProperties
            {
                HostName = "localhost",
                Username = "guest",
                Password = "guest"
            };
            var domainEventConsumer = 
                new DomainEventConsumerBase<MyEvent>(null, new NullLoggerFactory(), brokerProperties);
            var myEvent = new MyEvent(new MyDomainId(Guid.NewGuid()));
            await domainEventConsumer.Publish(myEvent);
        }

        [Fact]
        public async Task Can_Receive_DomainEvent_With_RabbitMQ_Muflone_Provider()
        {
            var brokerProperties = new BrokerProperties
            {
                HostName = "localhost",
                Username = "guest",
                Password = "guest"
            };
            var myEventHandler = new MyEventHandler(new InMemoryPersister(), new NullLoggerFactory());
            var domainEventConsumer =
                new DomainEventConsumerBase<MyEvent>(myEventHandler, new NullLoggerFactory(), brokerProperties);
            var myEvent = new MyEvent(new MyDomainId(Guid.NewGuid()));
            await domainEventConsumer.Publish(myEvent);

            Thread.Sleep(1000);
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