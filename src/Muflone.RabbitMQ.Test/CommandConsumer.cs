using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Muflone.Messages.Commands;
using Muflone.RabbitMQ.Consumers;
using RabbitMQ.Client;

namespace Muflone.RabbitMQ.Test
{
    public class CommandConsumer<TCommand> : CommandConsumerBase<TCommand> where TCommand : Command
    {
        public CommandConsumer(IBusControl busControl,
            ICommandHandler<TCommand> commandHandler,
            ILoggerFactory loggerFactory) : base(busControl, commandHandler, loggerFactory)
        {
        }

        public CommandConsumer(ICommandHandler<TCommand> commandHandler,
            ILoggerFactory loggerFactory, IOptions<BrokerProperties> options) : base(commandHandler, loggerFactory, options)
        {
        }

        public override Task Consume(CancellationToken cancellationToken = default)
        {
            // this.BusControl.RabbitMQChannel.BasicConsume(typeof(TCommand).Name, true, this.RabbitMQConsumer);

            return Task.CompletedTask;
        }
    }
}