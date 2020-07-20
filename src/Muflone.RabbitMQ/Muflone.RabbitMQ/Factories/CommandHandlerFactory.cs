using System;
using Muflone.Messages.Commands;
using Microsoft.Extensions.DependencyInjection;
using Muflone.RabbitMQ.Abstracts;

namespace Muflone.RabbitMQ.Factories
{
    public class CommandHandlerFactory : ICommandHandlerFactory
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
    }
}