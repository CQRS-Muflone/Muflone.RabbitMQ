using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Muflone.Messages;
using Muflone.Messages.Commands;
using Muflone.RabbitMQ.Helpers;
using RabbitMQ.Client;

namespace Muflone.RabbitMQ
{
    public class ServiceBus : IHostedService, IServiceBus, IEventBus
    {
        private readonly IBusControl busControl;
        private readonly ILogger logger;

        public ServiceBus(IBusControl busControl,
            ILoggerFactory loggerFactory, IOptions<BrokerProperties> options)
        {
            this.busControl = busControl;
            this.logger = loggerFactory.CreateLogger(this.GetType());
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            return this.busControl.Start(cancellationToken);
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await this.busControl.Stop(cancellationToken);
        }

        public async Task Send<T>(T command) where T : class, ICommand
        {
            try
            {
                this.busControl.RabbitMQChannel.QueueDeclare(command.GetType().Name,
                    true, false, false, null);

                var messageBody = RabbitMqMappers.MapMufloneMessageToRabbitMq(command);
                this.busControl.RabbitMQChannel.BasicPublish("", command.GetType().Name, null, messageBody);

                this.logger.LogInformation($"ServiceBus: Sending command {command.GetType()} AggregateId: {command.AggregateId}");
            }
            catch (Exception ex)
            {
                this.logger.LogError($"ServiceBus: Error sending command {command.GetType()} AggregateId: {command.AggregateId}, Message: {ex.Message}, StackTrace: {ex.StackTrace}");
                throw;
            }
        }

        public async Task RegisterHandler<T>(Action<T> handler) where T : IMessage
        {
            await Task.Yield();
        }

        public async Task Publish(IMessage @event)
        {
            try
            {
                this.busControl.RabbitMQChannel.ExchangeDeclare(@event.GetType().Name, ExchangeType.Fanout);

                var messageBody = RabbitMqMappers.MapMufloneMessageToRabbitMq(@event);
                this.busControl.RabbitMQChannel.BasicPublish(@event.GetType().Name, "", false, null, messageBody);

                await Task.Yield();
            }
            catch (Exception ex)
            {
                this.logger.LogError($"StackTrace: {ex.StackTrace}, Source: {ex.Source}");
                throw new Exception($"StackTrace: {ex.StackTrace}, Source: {ex.Source}");
            }
        }
    }
}