using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Muflone.Messages.Commands;
using Muflone.RabbitMQ.Abstracts.Commands;
using Muflone.RabbitMQ.Helpers;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Muflone.RabbitMQ.Consumers
{
    public abstract class CommandConsumerBase<TCommand> : ICommandConsumer<TCommand> where TCommand: class, ICommand
    {
        protected readonly IBusControl BusControl;
        protected readonly ICommandHandler<TCommand> CommandHandler;
        protected readonly AsyncEventingBasicConsumer RabbitMQConsumer;
        protected IModel RabbitMQChannel;

        private readonly ILogger logger;

        protected CommandConsumerBase(IBusControl busControl,
            ICommandHandler<TCommand> commandHandler,
            ILoggerFactory loggerFactory)
        {
            this.BusControl = busControl ?? throw new NullReferenceException($"Value cannot be null. (Parameter '{nameof(busControl)}')");
            this.CommandHandler = commandHandler ?? throw new NullReferenceException($"Value cannot be null. (Parameter '{nameof(commandHandler)}')");
            this.logger = loggerFactory.CreateLogger(this.GetType());

            this.RabbitMQConsumer = RabbitMqFactories.CreateEventingBasicCosumer(this.BusControl.RabbitMQChannel);
            this.RabbitMQConsumer.Received += this.CommandConsumer;
        }

        protected CommandConsumerBase(ICommandHandler<TCommand> commandHandler,
            ILoggerFactory loggerFactory, IOptions<BrokerProperties> options)
        {
            this.CommandHandler = commandHandler ?? throw new NullReferenceException($"Value cannot be null. (Parameter '{nameof(commandHandler)}')");
            this.logger = loggerFactory.CreateLogger(this.GetType());

            var connectionFactory = RabbitMqFactories.CreateConnectionFactory(options.Value);
            var connection = RabbitMqFactories.CreateConnection(connectionFactory);
            this.RabbitMQChannel = RabbitMqFactories.CreateChannel(connection);

            this.RabbitMQConsumer = RabbitMqFactories.CreateEventingBasicCosumer(this.RabbitMQChannel);
            this.RabbitMQConsumer.Received += this.CommandConsumer;
        }

        public abstract Task Consume(CancellationToken cancellationToken = default);

        protected async Task CommandConsumer(object sender, BasicDeliverEventArgs e)
        {
            try
            {
                var mufloneCommand = RabbitMqMappers.MapRabbitMqMessageToMuflone<TCommand>(e.Body);
                await this.CommandHandler.Handle(mufloneCommand);
                //using (var handler = this.CommandHandler)
                //    await handler.Handle(mufloneCommand);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, $"Original Message Received: {e.Body}");
            }
        }
    }
}