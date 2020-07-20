using System;
using Microsoft.Extensions.DependencyInjection;
using Muflone.Messages;
using Muflone.RabbitMQ.Abstracts;

namespace Muflone.RabbitMQ.Factories
{
    public class MessageHandlerFactory : IMessageHandlerFactory
    {
        private readonly IServiceProvider serviceProvider;

        public MessageHandlerFactory(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public IMessageHandler<T> GetMessageHandler<T>() where T : class, IMessage
        {
            return serviceProvider.GetService<IMessageHandler<T>>();
        }
    }
}