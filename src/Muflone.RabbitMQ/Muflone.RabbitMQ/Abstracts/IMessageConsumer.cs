using System.Threading;
using System.Threading.Tasks;
using Muflone.Messages;

namespace Muflone.RabbitMQ.Abstracts
{
    public interface IMessageConsumer<in TMessage> where TMessage : IMessage
    {
        Task Consume(CancellationToken cancellationToken = default);
    }
}