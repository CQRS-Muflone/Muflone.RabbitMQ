using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
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
            ILoggerFactory loggerFactory)
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

        public Task Send<T>(T command) where T : class, ICommand
        {
            try
            {
                var messageBody = RabbitMqMappers.MapMufloneMessageToRabbitMq(command);
                this.busControl.RabbitMQChannel.BasicPublish("", command.GetType().Name, null, messageBody);

                this.logger.LogInformation($"ServiceBus: Sending command {command.GetType()} AggregateId: {command.AggregateId}");
                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                this.logger.LogError($"ServiceBus: Error sending command {command.GetType()} AggregateId: {command.AggregateId}, Message: {ex.Message}, StackTrace: {ex.StackTrace}");
                throw;
            }
        }

        public Task Publish(IMessage @event)
        {
            try
            {
                this.busControl.RabbitMQChannel.ExchangeDeclare(@event.GetType().Name, ExchangeType.Fanout);

                var messageBody = RabbitMqMappers.MapMufloneMessageToRabbitMq(@event);
                this.busControl.RabbitMQChannel.BasicPublish(@event.GetType().Name, "", false, null, messageBody);

                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                this.logger.LogError($"StackTrace: {ex.StackTrace}, Source: {ex.Source}");
                throw;
            }
        }

        [Obsolete("With RabbitMQ, handlers must be registered in the busControl")]
        public Task RegisterHandler<T>(Action<T> handler) where T : IMessage
        {
            throw new Exception("With RabbitMQ, handlers must be registered in the busControl");
        }
    }
}