﻿using System;
using System.Text;
using Muflone.Messages;
using Newtonsoft.Json;

namespace Muflone.RabbitMQ
{
    public static class RabbitMqMappers
    {
        internal static byte[] MapMufloneMessageToRabbitMq(IMessage message) =>
            Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message));

        internal static TMessage MapRabbitMqMessageToMuflone<TMessage>(ReadOnlyMemory<byte> rabbitMqMessage) where TMessage : class, IMessage
        {
            var messageBody = Encoding.UTF8.GetString(rabbitMqMessage.ToArray());

            return JsonConvert.DeserializeObject<TMessage>(messageBody);
        }
    }
}