using Muflone.Messages.Commands;

namespace Muflone.RabbitMQ.Abstracts
{
    public interface ICommandHandlerFactory
    {
        ICommandHandler<T> GetCommandHandler<T>() where T : class, ICommand;
    }
}