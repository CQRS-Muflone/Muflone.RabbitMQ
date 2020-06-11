using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Muflone.RabbitMQ
{
    public static class MufloneRabbitMQHelper
    {
        public static IServiceCollection AddMufloneRabbitMQ(this IServiceCollection services, IOptions<BrokerProperties> options)
        {
            services.AddSingleton<IBusControl>(provider =>
            {
                var cancellationTokenSource = new CancellationTokenSource();
                var cancellationToken = cancellationTokenSource.Token;

                var busControl = new BusControl(options);
                busControl.Start(cancellationToken).GetAwaiter().GetResult();

                return busControl;
            });

            services.AddSingleton<IServiceBus, ServiceBus>();
            services.AddSingleton<IEventBus, ServiceBus>();
            services.AddSingleton<IHostedService, ServiceBus>();

            return services;
        }
    }
}