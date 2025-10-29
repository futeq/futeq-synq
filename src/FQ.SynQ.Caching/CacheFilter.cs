using System.Collections.Concurrent;
using Microsoft.Extensions.Options;

namespace FQ.SynQ.Caching;

internal sealed class CacheFilter<T, TOut> : IFilter<T, TOut>
    where T : IMessage<TOut>
{
    private static readonly ConcurrentDictionary<string, SemaphoreSlim> _locks = new();

    private readonly ICacheStore _store;
    private readonly ICacheSerializer _ser;
    private readonly IOptions<AskCacheOptions> _opt;
    private readonly ICachePartitionAccessor? _partition;

    public CacheFilter(ICacheStore store, ICacheSerializer ser, IOptions<AskCacheOptions> opt, ICachePartitionAccessor? partition = null)
    {
        _store = store; _ser = ser; _opt = opt; _partition = partition;
    }

    public async Task<TOut> Invoke(T message, CancellationToken ct, Next<TOut> next)
    {
        // Only for Ask<T>
        var isAsk = typeof(T).GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IAsk<>));
        
        if (!isAsk)
        {
            return await next(ct);
        }

        // Must implement ICacheable
        if (message is not ICacheable cacheable || string.IsNullOrWhiteSpace(cacheable.CacheKey))
        {
            return await next(ct);
        }

        // Global bypass or per-message bypass
        if (_opt.Value.GlobalBypass?.Invoke(message!) == true || cacheable.BypassCaching)
        {
            return await next(ct);
        }

        // Optional extra predicate
        if (_opt.Value.AdditionalAskPredicate is not null && !_opt.Value.AdditionalAskPredicate(typeof(T)))
            return await next(ct);

        var key = BuildKey(cacheable.CacheKey, typeof(T), _partition?.GetPartition());

        // Try get
        var (found, payload, _) = await _store.TryGetAsync(key, ct);
        
        if (found && payload is not null)
        {
            var cached = _ser.Deserialize<TOut>(payload);
            // If TOut is reference type, ensure not null unless CacheNulls true
            if (cached is not null || cacheable.CacheNulls)
                return cached!;
        }

        // Stampede protection
        if (_opt.Value.StampedeProtection)
        {
            var gate = _locks.GetOrAdd(key, _ => new SemaphoreSlim(1,1));
            await gate.WaitAsync(ct);
            try
            {
                // re-check
                (found, payload, _) = await _store.TryGetAsync(key, ct);
                if (found && payload is not null)
                {
                    var cached2 = _ser.Deserialize<TOut>(payload);
                    if (cached2 is not null || cacheable.CacheNulls)
                        return cached2!;
                }

                var result = await next(ct);

                // Store (success path — we don’t know “success/failure” in value terms in Synq core)
                var ttl = cacheable.Ttl ?? _opt.Value.DefaultTtl;
                var bytes = _ser.Serialize(result);
                await _store.SetAsync(key, bytes, _opt.Value.ContentType, ttl, cacheable.Tags, ct);
                return result;
            }
            finally
            {
                gate.Release();
            }
        }
        else
        {
            var result = await next(ct);
            var ttl = cacheable.Ttl ?? _opt.Value.DefaultTtl;
            var bytes = _ser.Serialize(result);
            await _store.SetAsync(key, bytes, _opt.Value.ContentType, ttl, cacheable.Tags, ct);
            return result;
        }
    }

    private static string BuildKey(string userKey, Type messageType, string? partition)
        => partition is null ? $"synq:q:{messageType.FullName}|{userKey}"
                             : $"synq:q:{partition}|{messageType.FullName}|{userKey}";
}