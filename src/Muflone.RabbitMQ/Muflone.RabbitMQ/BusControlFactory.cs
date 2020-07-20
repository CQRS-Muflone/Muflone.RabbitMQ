using Microsoft.Extensions.Options;

namespace Muflone.RabbitMQ
{
    public class BusControlFactory
    {
        public BusControlFactory(IOptions<BrokerProperties> options)
        {

        }
    }
}