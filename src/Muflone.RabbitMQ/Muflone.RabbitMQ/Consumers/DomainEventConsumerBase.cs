﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Muflone.Messages.Events;
using Muflone.RabbitMQ.Abstracts.Events;
using Muflone.RabbitMQ.Helpers;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Muflone.RabbitMQ.Consumers
{
    public abstract class DomainEventConsumerBase<TEvent> : IDomainEventConsumer<TEvent> where TEvent: class, IDomainEvent
    {
        protected readonly IBusControl BusControl;
        protected readonly IDomainEventHandler<TEvent> EventHandler;
        protected readonly AsyncEventingBasicConsumer RabbitMQConsumer;
        protected readonly ILogger Logger;

        protected DomainEventConsumerBase(IBusControl busControl,
            IDomainEventHandler<TEvent> eventHandler,
            ILoggerFactory loggerFactory)
        {
            this.BusControl = busControl ?? throw new NullReferenceException($"Value cannot be null. (Parameter '{nameof(busControl)}')");
            this.EventHandler = eventHandler ?? throw new NullReferenceException($"Value cannot be null. (Parameter '{nameof(eventHandler)}')");
            this.Logger = loggerFactory.CreateLogger(this.GetType());

            if (this.BusControl.RabbitMQChannel == null)
            {
                var cancellationTokenSource = new CancellationTokenSource();
                var cancellationToken = cancellationTokenSource.Token;
                this.BusControl.Start(cancellationToken);
            }

            this.BusControl.RabbitMQChannel.QueueDeclare(typeof(TEvent).Name, true, false, false, null);
            this.BusControl.RabbitMQChannel.QueueBind(typeof(TEvent).Name,
                typeof(TEvent).Name,"");
            
            this.RabbitMQConsumer = RabbitMqFactories.CreateAsyncEventingBasicConsumer(this.BusControl.RabbitMQChannel);
            this.RabbitMQConsumer.Received += this.EventConsumer;
        }

        private async Task EventConsumer(object sender, BasicDeliverEventArgs e)
        {
            try
            {
                var mufloneEvent = RabbitMqMappers.MapRabbitMqMessageToMuflone<TEvent>(e.Body);
                await this.EventHandler.Handle(mufloneEvent);
            }
            catch (Exception ex)
            {
                this.Logger.LogInformation($"Original message: {e.Body}");
                this.Logger.LogError($"StackTrace: {ex.StackTrace}, Source: {ex.Source}");
            }
        }

        public abstract Task Consume(CancellationToken cancellationToken = default);
    }
}