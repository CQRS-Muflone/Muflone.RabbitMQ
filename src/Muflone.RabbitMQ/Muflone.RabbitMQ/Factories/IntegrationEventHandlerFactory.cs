using System;
using Microsoft.Extensions.DependencyInjection;
using Muflone.Messages.Events;
using Muflone.RabbitMQ.Abstracts;

namespace Muflone.RabbitMQ.Factories
{
    public class IntegrationEventHandlerFactory : IIntegrationEventHandlerFactory
    {
        private readonly IServiceProvider serviceProvider;

        public IntegrationEventHandlerFactory(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public IIntegrationEventHandler<T> GetIntegrationEventHandler<T>() where T : class, IIntegrationEvent =>
            serviceProvider.GetService<IIntegrationEventHandler<T>>();
    }
}