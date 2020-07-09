using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Muflone.RabbitMQ.Test
{
    public abstract class TestBase
    {
        protected IServiceProvider ServiceProvider;

        protected TestBase()
        {
            var services = new ServiceCollection();

            var options = Options.Create(new BrokerProperties
            {
                HostName = "localhost",
                Username = "guest",
                Password = "guest",
                DispatchConsumersAsync = true
            });

            services.AddLogging();

            this.ServiceProvider = services.BuildServiceProvider();
        }
    }
}