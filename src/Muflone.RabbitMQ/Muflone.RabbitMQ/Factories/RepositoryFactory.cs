using System;
using Microsoft.Extensions.DependencyInjection;
using Muflone.Persistence;
using Muflone.RabbitMQ.Abstracts;

namespace Muflone.RabbitMQ.Factories
{
    public class RepositoryFactory : IRepositoryFactory
    {
        private readonly IServiceProvider serviceProvider;

        public RepositoryFactory(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public IRepository GetRepository() => serviceProvider.GetService<IRepository>();
    }
}