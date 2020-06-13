using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Muflone.Messages;
using Muflone.Messages.Commands;
using Muflone.RabbitMQ.Abstracts;
using Muflone.RabbitMQ.Helpers;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Muflone.RabbitMQ
{
    public sealed class BusControl : IBusControl
    {
        private readonly BrokerProperties brokerProperties;

        private AsyncEventingBasicConsumer rabbitMQConsumer;
        private readonly ILogger logger;

        public IModel RabbitMQChannel { get; private set; }

        public BusControl(IOptions<BrokerProperties> options,
            ILoggerFactory loggerFactory)
        {
            this.brokerProperties = options.Value;
            this.logger = loggerFactory.CreateLogger(this.GetType());
        }

        public Task Start(CancellationToken cancellationToken = default)
        {
            if (cancellationToken.IsCancellationRequested)
                cancellationToken.ThrowIfCancellationRequested();

            var connectionFactory = RabbitMqFactories.CreateConnectionFactory(this.brokerProperties);
            var connection = RabbitMqFactories.CreateConnection(connectionFactory);

            this.RabbitMQChannel = RabbitMqFactories.CreateChannel(connection);

            return Task.CompletedTask;
        }

        public Task Stop(CancellationToken cancellationToken = default)
        {
            if (cancellationToken.IsCancellationRequested)
                cancellationToken.ThrowIfCancellationRequested();

            if (!this.RabbitMQChannel.IsClosed)
                this.RabbitMQChannel.Close();

            this.RabbitMQChannel.Dispose();

            return Task.CompletedTask;
        }

        IConcurrencyDictionary<Type, ICommandHandler<T>>
            //Andare a vedere ServibUsi inmemory di ale

            private readonly Dictionary<Type, List<Action<Message>>> routes = new Dictionary<Type, List<Action<Message>>>();

            public void RegisterHandler<T>(Action<T> handler) where T : Message
            {
                try
                {
                    if (!routes.TryGetValue(typeof(T), out var handlers))
                    {
                        handlers = new List<Action<Message>>();
                        routes.Add(typeof(T), handlers);
                    }
                    handlers.Add(x => handler((T)x));
                }
                catch (Exception ex)
                {
                    log.Error(m => m($"RegisterHandler: {ex.Message}"), ex);
                    throw;
                }
            }

        public async Task RegisterConsumer<T>(Action<T> handler, CancellationToken cancellationToken) where T : IMessage
        {
            if (cancellationToken.IsCancellationRequested)
                cancellationToken.ThrowIfCancellationRequested();


            //Lista dei commamndHandler

            //Verifico non esista già la coda, bla bla
            this.RabbitMQChannel.QueueDeclare(typeof(TCommand).Name, true, false, false, null);

            this.rabbitMQConsumer = RabbitMqFactories.CreateAsyncEventingBasicConsumer(this.RabbitMQChannel);
            this.rabbitMQConsumer.Received += this.CommandConsumer;

            this.RabbitMQChannel.BasicConsume(typeof(TCommand).Name, true, this.rabbitMQConsumer);
        }
        private async Task CommandConsumer(object sender, BasicDeliverEventArgs @event)
        {
            try
            {
                var mufloneCommand = RabbitMqMappers.MapRabbitMqMessageToMuflone<TCommand>(@event.Body);

                //Chiamo l'ahndler corretto dalla mia lista, basata sul tipo

                //A questo punto, forse, non ha più senso fare un decoratro di Muflone.Command.CommandHandler, am chiamarlo direttamente
                await this.CommandHandler.Handle(mufloneCommand);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, $"Original Message Received: {@event.Body}");
                throw;
            }
        }


    }
}