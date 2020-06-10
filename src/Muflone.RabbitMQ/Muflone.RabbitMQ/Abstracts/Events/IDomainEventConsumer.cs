﻿using System.Threading;
using System.Threading.Tasks;
using Muflone.Messages.Events;

namespace Muflone.RabbitMQ.Abstracts.Events
{
    public interface IDomainEventConsumer<in TEvent> where TEvent : IDomainEvent
    {
        Task Consume(CancellationToken cancellationToken = default);
    }
}
