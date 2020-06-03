using System.Threading;
using System.Threading.Tasks;
using Muflone.Messages.Commands;

namespace Muflone.RabbitMQ.Abstracts.Commands
{
    public interface ICommandConsumer<in TCommand> where TCommand : ICommand
    {
        Task Consume(CancellationToken cancellationToken = default);
    }
}