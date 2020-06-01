using System;
using System.Text;
using Muflone.Messages.Commands;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Muflone.RabbitMQ.Factories
{
    internal static class RabbitMqFactories
    {
        internal static ConnectionFactory CreateConnectionFactory(BrokerProperties brokerProperties)
        {
            if (brokerProperties == null)
                throw new ArgumentNullException(nameof(brokerProperties));

            return new ConnectionFactory
            {
                HostName = brokerProperties.HostName,
                UserName = brokerProperties.Username,
                Password = brokerProperties.Password
            };
        }

        internal static IConnection CreateConnection(ConnectionFactory connectionFactory) =>
            connectionFactory.CreateConnection();

        internal static IModel CreateChannel(IConnection connection) => connection.CreateModel();

        internal static EventingBasicConsumer CreateEventingBasicCosumer(IModel channel) =>
            new EventingBasicConsumer(channel);

        internal static IModel CreateChannel(BrokerProperties brokerProperties)
        {
            if (brokerProperties == null)
                throw new ArgumentNullException(nameof(brokerProperties));

            var connectionFactory = new ConnectionFactory
            {
                HostName = brokerProperties.HostName,
                UserName = brokerProperties.Username,
                Password = brokerProperties.Password
            };

            using (var connection = connectionFactory.CreateConnection())
            {
                return connection.CreateModel();
            }
        }
    }
}