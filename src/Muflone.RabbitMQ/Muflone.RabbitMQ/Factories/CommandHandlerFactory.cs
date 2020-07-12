using System;
using System.Collections.Generic;
using Muflone.Messages.Commands;
using Microsoft.Extensions.DependencyInjection;

namespace Muflone.RabbitMQ.Factories
{
    public class CommandHandlerFactory
    {
        private readonly IServiceProvider serviceProvider;

        public CommandHandlerFactory(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public ICommandHandler<T> GetCommandHandler<T>() where T : class, ICommand
        {
            return serviceProvider.GetService<ICommandHandler<T>>();
        }

        public IEnumerable<ICommandHandler<T>> GetCommandHandlers<T>(Type handlerType) where T : class, ICommand =>
            serviceProvider.GetServices(handlerType) as IEnumerable<ICommandHandler<T>>;
    }
}