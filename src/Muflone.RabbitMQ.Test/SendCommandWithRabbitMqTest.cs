using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Muflone.Core;
using Muflone.Messages.Commands;
using Muflone.Persistence;
using Xunit;

namespace Muflone.RabbitMQ.Test
{
    public class SendCommandWithRabbitMqTest
    {
        private readonly IBusControl busControl;

        public SendCommandWithRabbitMqTest()
        {
            var options = Options.Create(new BrokerProperties
            {
                HostName = "localhost",
                Username = "guest",
                Password = "guest"
            });
            this.busControl = new BusControl(options);
        }

        [Fact]
        public void Cannot_Create_CommandConsumer_Without_BusControl()
        {
            var exception =
                Assert.ThrowsAny<Exception>(() =>
                    new CommandConsumer<MyCommand>(null, null, new NullLoggerFactory()));

            Assert.Equal("Value cannot be null. (Parameter 'busControl')", exception.Message);
        }

        [Fact]
        public void Cannot_Create_CommandConsumer_Without_CommandHandler()
        {
            var exception =
                Assert.ThrowsAny<Exception>(() =>
                    new CommandConsumer<MyCommand>(this.busControl, null, new NullLoggerFactory()));

            Assert.Equal("Value cannot be null. (Parameter 'commandHandler')", exception.Message);
        }

        [Fact]
        public async Task Can_Send_Command_With_Servicebus_Muflone_Provider()
        {
            var options = Options.Create(new BrokerProperties
            {
                HostName = "localhost",
                Username = "guest",
                Password = "guest"
            });
            var serviceBus = new ServiceBus(this.busControl, new NullLoggerFactory(), options);

            var myCommand = new MyCommand(new MyDomainId(Guid.NewGuid()));
            var mySecondCommand = new MySecondCommand(new MyDomainId(Guid.NewGuid()));
            
            await serviceBus.Send(myCommand);
            await serviceBus.Send(mySecondCommand);
        }

        [Fact]
        public async Task Can_Receive_Command_With_RabbitMQ_Muflone_Provider()
        {
            var options = Options.Create(new BrokerProperties
            {
                HostName = "localhost",
                Username = "guest",
                Password = "guest"
            });
            var myCommandHandler = new MyCommandCommandHandler(new InMemoryRepository(), new NullLoggerFactory());
            var commandConsumer =
                new CommandConsumer<MyCommand>(myCommandHandler, new NullLoggerFactory(), options);

            await commandConsumer.Consume(new CancellationToken(false));

            Thread.Sleep(1000);
            Assert.Equal("I am a command", TestResult.CommandContent);
        }

        public class MyDomainId : DomainId
        {
            public MyDomainId(Guid value) : base(value)
            {
            }
        }

        public class MyCommand : Command
        {
            public readonly string CommandContent;

            public MyCommand(MyDomainId aggregateId, string who = "anonymous") : base(aggregateId, who)
            {
                this.CommandContent = "I am a command";
            }
        }

        public class MySecondCommand : Command
        {
            public readonly string CommandContent;

            public MySecondCommand(MyDomainId aggregateId, string who = "anonymous") : base(aggregateId, who)
            {
                this.CommandContent = "I am a command";
            }
        }

        public class MyCommandCommandHandler : CommandHandler<MyCommand>
        {
            public MyCommandCommandHandler(IRepository repository, ILoggerFactory loggerFactory) : base(repository, loggerFactory)
            {
            }

            public override Task Handle(MyCommand command)
            {
                TestResult.CommandContent = command.CommandContent;
                return Task.CompletedTask;
            }
        }

        public static class TestResult
        {
            public static string CommandContent;
        }
    }
}
