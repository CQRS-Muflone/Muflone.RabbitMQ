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

        public ICommandHandler<T> GetCommandHandlerAsync<T>() where T : class, ICommand
        {
            return serviceProvider.GetService<ICommandHandler<T>>();
        }

        public IEnumerable<ICommandHandler<ICommand>> GetCommandHandlers(Type handlerType) =>
            serviceProvider.GetServices(handlerType) as IEnumerable<ICommandHandler<ICommand>>;
    }
}