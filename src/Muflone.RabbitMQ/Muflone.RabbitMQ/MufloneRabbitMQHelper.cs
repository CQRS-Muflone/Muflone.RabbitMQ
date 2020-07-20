using System;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Muflone.Messages.Commands;
using Muflone.Persistence;
using Muflone.RabbitMQ.Abstracts;
using Muflone.RabbitMQ.Factories;

namespace Muflone.RabbitMQ
{
    public static class MufloneRabbitMQHelper
    {
        public static IServiceCollection AddMufloneRabbitMQ(this IServiceCollection services,
            IOptions<BrokerProperties> options, ISubscriberRegistry subscriberRegistry)
        {
            services.AddSingleton<IBusControl>(provider =>
            {
                var cancellationTokenSource = new CancellationTokenSource();
                var cancellationToken = cancellationTokenSource.Token;

                var messageHandlerFactory = new MessageHandlerFactory(provider);
                var commandHandlerFactory = new CommandHandlerFactory(provider);
                var domainEventHandlerFactory = new DomainEventHandlerFactory(provider);

                var busControl = new BusControl(subscriberRegistry,
                    provider,
                    options, new NullLoggerFactory());
                busControl.Start(cancellationToken).GetAwaiter().GetResult();

                foreach (var observer in subscriberRegistry.CommandObservers)
                {
                    busControl.RegisterCommandConsumers(observer.Key, cancellationToken);
                }

                return busControl;
            });

            services.AddSingleton<IServiceBus, ServiceBus>();
            services.AddSingleton<IEventBus, ServiceBus>();
            services.AddSingleton<IHostedService, ServiceBus>();

            return services;
        }
    }
}