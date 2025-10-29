namespace FQ.SynQ.Filters.DomainEvents; 

/// <summary>Publishes domain events after a successful command execution.</summary>
public sealed class DomainEventsFilter<T, TOut> : IFilter<T, TOut>
    where T : IMessage<TOut>
{
    private readonly IDomainEventsPublisher _publisher;

    public DomainEventsFilter(IDomainEventsPublisher publisher) => _publisher = publisher;

    public async Task<TOut> Invoke(T message, CancellationToken ct, Next<TOut> next)
    {
        var isAct = typeof(IAct).IsAssignableFrom(typeof(T)) ||
                    typeof(T).GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IAct<>));
        if (!isAct) return await next(ct);

        var result = await next(ct);
        await _publisher.PublishPendingAsync(ct);
        return result;
    }
}