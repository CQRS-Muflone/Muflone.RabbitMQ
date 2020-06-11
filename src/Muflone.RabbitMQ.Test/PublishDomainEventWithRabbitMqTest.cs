﻿using System;
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

            this.busControl = new BusControl(options);
        }

        [Fact]
        public void Cannot_Create_DomainEventConsumer_Without_BusControl()
        {
            var exception =
                Assert.ThrowsAny<Exception>(() =>
                    new DomainEventConsumer<MyEvent>(null, null, new NullLoggerFactory()));

            Assert.Equal("Value cannot be null. (Parameter 'busControl')", exception.Message);
        }

        [Fact]
        public void Cannot_Create_DomainEventConsumer_Without_EventHandler()
        {
            var exception =
                Assert.ThrowsAny<Exception>(() =>
                    new DomainEventConsumer<MyEvent>(this.busControl, null, new NullLoggerFactory()));

            Assert.Equal("Value cannot be null. (Parameter 'eventHandler')", exception.Message);
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
            var serviceBus = new ServiceBus(this.busControl, new NullLoggerFactory(), options);

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
            var domainEventConsumer =
                new DomainEventConsumer<MyEvent>(this.busControl, myEventHandler, new NullLoggerFactory());

            var cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = cancellationTokenSource.Token;
            await domainEventConsumer.Consume(cancellationToken);

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