using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Muflone.Messages;
using Muflone.Messages.Commands;
using Muflone.RabbitMQ.Consumers;

namespace Muflone.RabbitMQ
{
    public static class MufloneRabbitMQHelper
    {
        public static IServiceCollection AddMufloneRabbitMQ(this IServiceCollection services, IOptions<BrokerProperties> options, ICollection<Action<CommandConsumer<IMessage>>> handlers)
        {
            services.AddSingleton<IBusControl>(provider =>
            {
                var cancellationTokenSource = new CancellationTokenSource();
                var cancellationToken = cancellationTokenSource.Token;

                var busControl = new BusControl(options);
                busControl.Start(cancellationToken).GetAwaiter().GetResult();

                foreach (var handler in handlers)
                    busControl.RegisterConsumer(handler, CancellationToken.None).Wait();

                return busControl;
            });

            services.AddSingleton<IServiceBus, ServiceBus>();
            services.AddSingleton<IEventBus, ServiceBus>();
            services.AddSingleton<IHostedService, ServiceBus>();

            return services;
        }
    }
}