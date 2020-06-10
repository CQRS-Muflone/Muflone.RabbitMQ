using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Muflone.Messages.Commands;
using Muflone.RabbitMQ.Abstracts.Commands;
using Muflone.RabbitMQ.Helpers;
using RabbitMQ.Client.Events;

namespace Muflone.RabbitMQ.Consumers
{
    public abstract class CommandConsumerBase<TCommand> : ICommandConsumer<TCommand> where TCommand: class, ICommand
    {
        protected readonly IBusControl BusControl;
        protected readonly ICommandHandler<TCommand> CommandHandler;
        protected readonly AsyncEventingBasicConsumer RabbitMQConsumer;

        protected readonly ILogger Logger;

        protected CommandConsumerBase(IBusControl busControl,
            ICommandHandler<TCommand> commandHandler,
            ILoggerFactory loggerFactory)
        {
            this.BusControl = busControl ?? throw new NullReferenceException($"Value cannot be null. (Parameter '{nameof(busControl)}')");
            this.CommandHandler = commandHandler ?? throw new NullReferenceException($"Value cannot be null. (Parameter '{nameof(commandHandler)}')");
            this.Logger = loggerFactory.CreateLogger(this.GetType());

            if (this.BusControl.RabbitMQChannel == null)
            {
                var cancellationTokenSource = new CancellationTokenSource();
                var cancellationToken = cancellationTokenSource.Token;
                this.BusControl.Start(cancellationToken);
            }

            this.BusControl.RabbitMQChannel.QueueDeclare(typeof(TCommand).Name, true, false, false, null);

            this.RabbitMQConsumer = RabbitMqFactories.CreateAsyncEventingBasicConsumer(this.BusControl.RabbitMQChannel);
            this.RabbitMQConsumer.Received += this.CommandConsumer;
        }

        private async Task CommandConsumer(object sender, BasicDeliverEventArgs @event)
        {
            try
            {
                var mufloneCommand = RabbitMqMappers.MapRabbitMqMessageToMuflone<TCommand>(@event.Body);
                await this.CommandHandler.Handle(mufloneCommand);
            }
            catch (Exception ex)
            {
                this.Logger.LogError(ex, $"Original Message Received: {@event.Body}");
            }
        }

        public abstract Task Consume(CancellationToken cancellationToken = default);
    }
}