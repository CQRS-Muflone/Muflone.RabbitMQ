using Microsoft.Extensions.Logging;
using Muflone.Messages.Commands;
using Muflone.RabbitMQ.Consumers;

namespace Muflone.RabbitMQ.Test
{
    public class CommandConsumer<TCommand> : CommandConsumerBase<TCommand> where TCommand : Command
    {
        public CommandConsumer(IBusControl busControl,
            ICommandHandler<TCommand> commandHandler,
            ILoggerFactory loggerFactory) : base(busControl, commandHandler, loggerFactory)
        {
        }
    }
}