using System;
using System.Text;
using Muflone.Messages;
using Newtonsoft.Json;

namespace Muflone.RabbitMQ.Helpers
{
    public static class RabbitMqMappers
    {
        public static byte[] MapMufloneMessageToRabbitMq(IMessage message) =>
            Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message));


        public static object MapRabbitMqMessageToMuflone(Type type, ReadOnlyMemory<byte> rabbitMqMessage) 
        {
            var messageBody = Encoding.UTF8.GetString(rabbitMqMessage.ToArray());

            return JsonConvert.DeserializeObject(messageBody, type);
        }

        public static TMessage MapRabbitMqMessageToMuflone<TMessage>(ReadOnlyMemory<byte> rabbitMqMessage) where TMessage : IMessage
        {
            var messageBody = Encoding.UTF8.GetString(rabbitMqMessage.ToArray());

            return JsonConvert.DeserializeObject<TMessage>(messageBody);
        }
    }
}