using System;
using Microsoft.Extensions.DependencyInjection;
using Muflone.Messages;

namespace Muflone.RabbitMQ.Factories
{
    public class MessageHandlerFactory
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