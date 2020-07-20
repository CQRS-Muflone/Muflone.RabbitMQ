using System;
using Microsoft.Extensions.DependencyInjection;
using Muflone.Messages.Events;
using Muflone.RabbitMQ.Abstracts;

namespace Muflone.RabbitMQ.Factories
{
    public class DomainEventHandlerFactory : IDomainEventHandlerFactory
    {
        private readonly IServiceProvider serviceProvider;

        public DomainEventHandlerFactory(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public IDomainEventHandler<T> GetDomainEventHandler<T>() where T : class, IDomainEvent =>
            serviceProvider.GetService<IDomainEventHandler<T>>();
    }
}