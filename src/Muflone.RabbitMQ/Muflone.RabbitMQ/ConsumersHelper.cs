using System.Collections.Generic;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using Muflone.Messages.Commands;
using Muflone.RabbitMQ.Abstracts.Commands;

namespace Muflone.RabbitMQ
{
    public static class ConsumersHelper
    {
        public static IServiceCollection AddCommandConsumers<TCommand>(this IServiceCollection services,
            IEnumerable<ICommandConsumer<TCommand>> commandConsumers) where TCommand : Command
        {
            var cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = cancellationTokenSource.Token;

            foreach (var commandConsumer in commandConsumers)
            {
                commandConsumer.Consume(cancellationToken).GetAwaiter().GetResult();
            }
            return services;
        }
    }
}