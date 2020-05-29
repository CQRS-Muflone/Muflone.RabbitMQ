using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Muflone.Core;
using Muflone.Messages.Commands;
using Muflone.Persistence;
using Xunit;

namespace Muflone.RabbitMQ.Test
{
    public class SendCommandWithRabbitMqTest
    {
        [Fact]
        public void Cannot_Create_CommandConsumer_Without_BrokerProperties()
        {
            var myCommandHandler = new MyCommandCommandHandler(new InMemoryRepository(), new NullLoggerFactory());

            var exception =
                Assert.ThrowsAny<Exception>(() =>
                    new RabbitMqCommandConsumer<MyCommand>(myCommandHandler, new NullLoggerFactory(), null));

            Assert.Equal("Value cannot be null. (Parameter 'brokerProperties')", exception.Message);
        }

        [Fact]
        public async Task Can_Send_Command_With_RabbitMQ_Muflone_Provider()
        {
            var brokerProperties = new BrokerProperties
            {
                HostName = "localhost",
                Username = "guest",
                Password = "guest"
            };
            var myCommandHandler = new MyCommandCommandHandler(new InMemoryRepository(), new NullLoggerFactory());
            var commandConsumer =
                new RabbitMqCommandConsumer<MyCommand>(myCommandHandler, new NullLoggerFactory(), brokerProperties);
            
            var myCommand = new MyCommand(new MyDomainId(Guid.NewGuid()));
            await commandConsumer.Send(myCommand, new CancellationToken(false));
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

        public class MyCommandCommandHandler : CommandHandler<MyCommand>
        {
            public MyCommandCommandHandler(IRepository repository, ILoggerFactory loggerFactory) : base(repository, loggerFactory)
            {
            }

            public override Task Handle(MyCommand command)
            {
                return Task.CompletedTask;
            }
        }
    }
}
