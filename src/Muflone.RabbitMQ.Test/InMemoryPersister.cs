using System.Threading.Tasks;
using Muflone.Persistence;

namespace Muflone.RabbitMQ.Test
{
    public class InMemoryPersister : IPersister
    {
        public Task<T> GetBy<T>(string id) where T : class
        {
            throw new System.NotImplementedException();
        }

        public Task Insert<T>(T entity) where T : class
        {
            throw new System.NotImplementedException();
        }

        public Task Update<T>(T entity) where T : class
        {
            throw new System.NotImplementedException();
        }

        public Task Delete<T>(T entity) where T : class
        {
            throw new System.NotImplementedException();
        }
    }
}