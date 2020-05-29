using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Muflone.Messages.Commands;
using Newtonsoft.Json;
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
            if (brokerProperties == null)
                throw new ArgumentNullException(nameof(brokerProperties));

            var connectionFactory = new ConnectionFactory
            {
                HostName = brokerProperties.HostName,
                UserName = brokerProperties.Username,
                Password = brokerProperties.Password
            };

            this.logger = loggerFactory.CreateLogger(this.GetType());

            var connection = connectionFactory.CreateConnection();

            this.rabbitMqChannel = connection.CreateModel();
            this.rabbitMqChannel.QueueDeclare(typeof(TCommand).Name, true, false, false, null);

            if (commandHandler == null)
                return;

            this.commandHandler = commandHandler;

            var rabbitMqConsumer = new EventingBasicConsumer(this.rabbitMqChannel);
            rabbitMqConsumer.Received += this.CommandConsumer;

            this.rabbitMqChannel.BasicConsume(typeof(TCommand).Name, true, rabbitMqConsumer);
        }

        private void CommandConsumer(object sender, BasicDeliverEventArgs e)
        {
            try
            {
                var mufloneCommand = MapRabbitMqMessageToMuflone(e.Body);
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
                var messageBody = MapMufloneMessageToRabbitMq(command);
                this.rabbitMqChannel.BasicPublish("", typeof(TCommand).Name, null, messageBody);

                await Task.Yield();
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, $"Original Message to Send: {command}");
            }
        }

        #region Helpers
        private static byte[] MapMufloneMessageToRabbitMq(TCommand command) =>
            Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(command));

        private static TCommand MapRabbitMqMessageToMuflone(ReadOnlyMemory<byte> rabbitMqMessage)
        {
            var messageBody = Encoding.UTF8.GetString(rabbitMqMessage.ToArray());

            return JsonConvert.DeserializeObject<TCommand>(messageBody);
        }
        #endregion
    }
}