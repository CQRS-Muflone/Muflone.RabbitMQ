using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Muflone.RabbitMQ
{
    public static class BrokerHelper
    {
        public static IServiceCollection AddMufloneWithRabbitMQ(this IServiceCollection services)
        {


            services.AddSingleton<IServiceBus, ServiceBus>();
            services.AddSingleton<IEventBus, ServiceBus>();
            services.AddSingleton<IHostedService, ServiceBus>();

            return services;
        }
    }
}