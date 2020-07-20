using Muflone.Persistence;

namespace Muflone.RabbitMQ.Abstracts
{
    public interface IRepositoryFactory
    {
        IRepository GetRepository();
    }
}