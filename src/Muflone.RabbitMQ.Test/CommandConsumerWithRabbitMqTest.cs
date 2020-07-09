using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Muflone.Core;
using Muflone.Messages.Commands;
using Muflone.Persistence;
using Xunit;

namespace Muflone.RabbitMQ.Test
{
    public class CommandConsumerWithRabbitMqTest : TestBase
    {
        private readonly IBusControl busControl;

        public CommandConsumerWithRabbitMqTest()
        {
            this.busControl = this.ServiceProvider.GetService<IBusControl>();
        }

        [Fact]
        public async Task Can_Send_Command_With_Servicebus_Muflone_Provider()
        {
            var serviceBus = this.ServiceProvider.GetService<IServiceBus>();

            var myCommand = new MyCommand(new MyDomainId(Guid.NewGuid()));

            await serviceBus.Send(myCommand);
        }

        [Fact]
        public async Task Can_Receive_Command_With_RabbitMQ_Muflone_Provider()
        {
            var myCommandHandler = new MyCommandCommandHandler(new InMemoryRepository(), new NullLoggerFactory());
            //var commandConsumer =
            //    new CommandConsumer<MyCommand>(this.busControl, myCommandHandler, new NullLoggerFactory());

            //var cancellationTokenSource = new CancellationTokenSource();
            //var cancellationToken = cancellationTokenSource.Token;

            //await this.busControl.RegisterConsumer(commandConsumer, cancellationToken);

            Thread.Sleep(2000);
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
