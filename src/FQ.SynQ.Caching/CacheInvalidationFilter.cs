namespace FQ.SynQ.Caching;

internal sealed class CacheInvalidationFilter<T, TOut> : IFilter<T, TOut>
    where T : IMessage<TOut>
{
    private readonly ICacheStore _store;
    private readonly ICachePartitionAccessor? _partition;

    public CacheInvalidationFilter(ICacheStore store, ICachePartitionAccessor? partition = null)
    {
        _store = store; _partition = partition;
    }

    public async Task<TOut> Invoke(T message, CancellationToken ct, Next<TOut> next)
    {
        // Only for commands
        var isAct = typeof(IAct).IsAssignableFrom(typeof(T)) ||
                    typeof(T).GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IAct<>));

        if (!isAct) return await next(ct);

        var result = await next(ct); // let UoW/handler/domain events run

        if (message is ICacheInvalidation inv)
        {
            var partition = _partition?.GetPartition();
            
            // Keys: if provided, they should already be full keys; if they are user keys, you can decide to prefix here.
            if (inv.Keys is { Count: > 0 })
            {
                await _store.InvalidateByKeysAsync(inv.Keys, ct);
            }

            if (inv.Tags is { Count: > 0 })
            {
                // Tags usually don't need partition prefix (store impl should scope them if desired)
                await _store.InvalidateByTagsAsync(inv.Tags, ct);
            }
        }

        return result;
    }
}