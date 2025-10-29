using System.Text.Json;

namespace FQ.SynQ.Filters.Idempotency;

/// <summary>
/// Ensures that repeated executions of the same command with the same idempotency key
/// return the original result without re-running the handler.
/// </summary>
public sealed class IdempotencyFilter<T, TOut> : IFilter<T, TOut>
    where T : IMessage<TOut>
{
    private readonly IIdempotencyKeyAccessor _keyAccessor;
    private readonly IIdempotencyStore _store;
    private readonly JsonSerializerOptions _json;
    private readonly TimeSpan _ttl;
    private readonly string _contentType;

    public IdempotencyFilter(
        IIdempotencyKeyAccessor keyAccessor,
        IIdempotencyStore store,
        JsonSerializerOptions? json = null,
        TimeSpan? ttl = null,
        string contentType = "application/json")
    {
        _keyAccessor = keyAccessor;
        _store = store;
        _json = json ?? new JsonSerializerOptions(JsonSerializerDefaults.Web);
        _ttl = ttl ?? TimeSpan.FromMinutes(10);
        _contentType = contentType;
    }

    public async Task<TOut> Invoke(T message, CancellationToken ct, Next<TOut> next)
    {
        // Only for IAct / IAct<TOut>
        var isAct = typeof(IAct).IsAssignableFrom(typeof(T)) ||
                    typeof(T).GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IAct<>));
        if (!isAct) return await next(ct);

        var key = _keyAccessor.GetKey();
        if (string.IsNullOrWhiteSpace(key)) return await next(ct);

        var msgType = typeof(T).FullName!;

        var (found, payload, _) = await _store.TryGetAsync(key, msgType, ct);
        if (found && payload is not null)
        {
            var restored = JsonSerializer.Deserialize<TOut>(payload, _json);
            if (restored is not null) return restored;
            // Fallthrough on deser fail
        }

        var result = await next(ct);

        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(result, _json);
        await _store.SetAsync(key, msgType, bytes, _contentType, _ttl, ct);

        return result;
    }
}