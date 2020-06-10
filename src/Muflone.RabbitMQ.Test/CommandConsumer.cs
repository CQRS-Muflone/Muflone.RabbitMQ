using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
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

        public override Task Consume(CancellationToken cancellationToken = default)
        {
            this.BusControl.RabbitMQChannel.BasicConsume(typeof(TCommand).Name, true, this.RabbitMQConsumer);

            return Task.CompletedTask;
        }
    }
}