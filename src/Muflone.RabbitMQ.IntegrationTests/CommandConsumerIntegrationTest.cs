using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Muflone.Core;
using Muflone.Messages;
using Muflone.Messages.Commands;
using Muflone.Persistence;
using Muflone.RabbitMQ.Abstracts;
using Xunit;

namespace Muflone.RabbitMQ.IntegrationTests
{
    public class CommandConsumerIntegrationTest
    {
        private readonly IServiceProvider _serviceProvider;

        public CommandConsumerIntegrationTest()
        {
            var services = new ServiceCollection();

            var options = Options.Create(new BrokerProperties
            {
                HostName = "localhost",
                Username = "guest",
                Password = "guest",
                DispatchConsumersAsync = true
            });
            services.AddSingleton<IRepository, InMemoryRepository>();

            services.AddLogging();

            services.AddScoped<ICommandHandler<MyCommand>, MyCommandCommandHandler>();

            var subscriberRegistry = new SubscriberRegistry();
            subscriberRegistry.Register<MyCommand, MyCommandCommandHandler>();

            services.AddMufloneRabbitMQ(options, subscriberRegistry);

            this._serviceProvider = services.BuildServiceProvider();
        }

        [Fact]
        public void Can_Resolve_CommandHandler()
        {
            var commandHandler = this._serviceProvider.GetService<ICommandHandler<MyCommand>>();

            Assert.NotNull(commandHandler);
        }

        [Fact]
        public async Task Can_SendCommand_Through_RMQ_Broker()
        {
            var serviceBus = this._serviceProvider.GetService<IServiceBus>();

            var myCommand = new MyCommand(new MyDomainId(Guid.NewGuid()));
            await serviceBus.Send(myCommand);

            Thread.Sleep(5000);
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
                TestResult.CommandContent = command.CommandContent;
                return Task.CompletedTask;
            }
        }

        public static class TestResult
        {
            public static string CommandContent;
        }

        public class MessageHandlerFactory : IMessageHandlerFactory
        {
            private readonly IServiceProvider _serviceProvider;

            public MessageHandlerFactory(IServiceProvider serviceProvider)
            {
                this._serviceProvider = serviceProvider;
            }

            public IMessageHandler GetMessageHandler(Type handlerType)
            {
                return this._serviceProvider.GetService<IMessageHandler<MyCommand>>();
            }
        }
    }
}
