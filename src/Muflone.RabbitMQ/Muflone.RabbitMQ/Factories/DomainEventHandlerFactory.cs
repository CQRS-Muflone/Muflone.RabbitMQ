using System;
using Microsoft.Extensions.DependencyInjection;
using Muflone.Messages.Events;

namespace Muflone.RabbitMQ.Factories
{
    public class DomainEventHandlerFactory
    {
        private readonly IServiceProvider serviceProvider;

        public DomainEventHandlerFactory(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public IDomainEventHandler<T> GetDomainEventHandler<T>() where T : class, IDomainEvent
        {
            return serviceProvider.GetService<IDomainEventHandler<T>>();
        }
    }
}