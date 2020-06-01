using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Muflone.Messages.Commands;
using Muflone.RabbitMQ.Helpers;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Muflone.RabbitMQ
{
    public class RabbitMqCommandConsumer<TCommand> : ICommandConsumer<TCommand> where TCommand: class, ICommand
    {
        private readonly ICommandHandler<TCommand> commandHandler;
        private readonly ILogger logger;
        private readonly IModel rabbitMqChannel;

        public RabbitMqCommandConsumer(ICommandHandler<TCommand> commandHandler,
            ILoggerFactory loggerFactory, BrokerProperties brokerProperties)
        {
            this.logger = loggerFactory.CreateLogger(this.GetType());

            var connectionFactory = RabbitMqFactories.CreateConnectionFactory(brokerProperties);
            var connection = RabbitMqFactories.CreateConnection(connectionFactory);
            this.rabbitMqChannel = RabbitMqFactories.CreateChannel(connection);

            this.rabbitMqChannel.QueueDeclare(typeof(TCommand).Name, true, false, false, null);

            if (commandHandler == null)
                return;

            this.commandHandler = commandHandler;

            var rabbitMqConsumer = RabbitMqFactories.CreateEventingBasicCosumer(this.rabbitMqChannel);
            rabbitMqConsumer.Received += this.CommandConsumer;
            this.rabbitMqChannel.BasicConsume(typeof(TCommand).Name, true, rabbitMqConsumer);
        }

        private void CommandConsumer(object sender, BasicDeliverEventArgs e)
        {
            try
            {
                var mufloneCommand = RabbitMqMappers.MapRabbitMqMessageToMuflone<TCommand>(e.Body);
                this.commandHandler.Handle(mufloneCommand);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, $"Original Message Received: {e.Body}");
            }
        }

        public async Task Send(TCommand command, CancellationToken cancellationToken = default)
        {
            try
            {
                var messageBody = RabbitMqMappers.MapMufloneMessageToRabbitMq(command);
                this.rabbitMqChannel.BasicPublish("", typeof(TCommand).Name, null, messageBody);

                await Task.Yield();
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, $"Original Message to Send: {command}");
            }
        }
    }
}