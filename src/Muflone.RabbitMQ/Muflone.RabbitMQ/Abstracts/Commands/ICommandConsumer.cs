using Muflone.Messages;

namespace Muflone.RabbitMQ.Abstracts.Commands
{
    public interface ICommandConsumer<in TCommand> : IMessageConsumer<TCommand> where TCommand : IMessage
    {
    }
}