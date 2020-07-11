using System;
using System.Linq;
using Microsoft.Extensions.Logging;
using Muflone.RabbitMQ.Abstracts;

namespace Muflone.RabbitMQ
{
    public class MessageProcessor
    {
        private readonly ILogger logger;

        private readonly ISubscriberRegistry subscriberRegistry;
        private readonly IServiceProvider serviceProvider;

        public MessageProcessor(ISubscriberRegistry subscriberRegistry,
            IServiceProvider serviceProvider,
            IBusControl busControl, IServiceBus serviceBus, IEventBus eventBus,
            ILoggerFactory loggerFactory)
        {
            if (subscriberRegistry == null)
                throw new NullReferenceException(nameof(subscriberRegistry));

            if (subscriberRegistry.Observers == null || !subscriberRegistry.Observers.Any())
                throw new Exception("No handlers found! At least one handler for message is required!");

            this.subscriberRegistry = subscriberRegistry;
            this.serviceProvider = serviceProvider;

            this.logger = loggerFactory.CreateLogger(GetType());
        }
    }
}