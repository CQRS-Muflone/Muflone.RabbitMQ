using Muflone.Messages.Commands;
using RabbitMQ.Client;

namespace Muflone.RabbitMQ.Factories
{
    public class RegisterCommandFactory<T> where T : class, ICommand
    {
        private IModel RabbitMQChannel { get; }
    }
}