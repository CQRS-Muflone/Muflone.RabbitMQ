using System.Threading;
using System.Threading.Tasks;

namespace Muflone.Messages.Commands
{
    public interface ICommandConsumer<in TCommand> where TCommand : ICommand
    {
        Task Send(TCommand command, CancellationToken cancellationToken = default);
    }
}