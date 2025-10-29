using System.Collections.Concurrent;

namespace FQ.SynQ.Caching.UnitTests;

public interface IClock
{
    DateTimeOffset UtcNow { get; }
}

public sealed class FakeClock : IClock
{
    public DateTimeOffset UtcNow { get; private set; } = DateTimeOffset.UtcNow;
    public void Advance(TimeSpan by)
    {
        UtcNow = UtcNow.Add(by);
    }
}

public sealed class TestPartitionAccessor : ICachePartitionAccessor
{
    private string? _partition;
    public void Set(string? p)
    {
        _partition = p;
    }
    public string? GetPartition()
    {
        return _partition;
    }
}

public sealed class InMemoryCacheStore : ICacheStore
{
    private readonly IClock _clock;

    private sealed record Entry(byte[] Payload, string ContentType, DateTimeOffset Expires, HashSet<string> Tags);

    private readonly ConcurrentDictionary<string, Entry> _data = new();
    private readonly ConcurrentDictionary<string, HashSet<string>> _tags = new(StringComparer.Ordinal);

    public InMemoryCacheStore(IClock clock)
    {
        _clock = clock;
    }

    public Task<(bool, byte[], string Empty)> TryGetAsync(string key, CancellationToken ct)
    {
        if (_data.TryGetValue(key, out var e))
        {
            if (e.Expires > _clock.UtcNow)
            {
                return Task.FromResult((true, e.Payload, e.ContentType));
            }
            else
            {
                _data.TryRemove(key, out _);
            }
        }

        return Task.FromResult((false, Array.Empty<byte>(), string.Empty));
    }

    public Task SetAsync(string key, byte[] payload, string contentType, TimeSpan ttl, IReadOnlyCollection<string>? tags, CancellationToken ct)
    {
        var entry = new Entry(payload, contentType, _clock.UtcNow.Add(ttl), new HashSet<string>(tags ?? Array.Empty<string>()));
        _data[key] = entry;

        foreach (var tag in entry.Tags)
        {
            var set = _tags.GetOrAdd(tag, _ => new HashSet<string>(StringComparer.Ordinal));
            lock (set)
            {
                set.Add(key);
            }
        }

        return Task.CompletedTask;
    }

    public Task InvalidateByKeysAsync(IEnumerable<string> keys, CancellationToken ct)
    {
        foreach (var key in keys)
        {
            if (_data.TryRemove(key, out var removed))
            {
                foreach (var tag in removed.Tags)
                {
                    if (_tags.TryGetValue(tag, out var set))
                    {
                        lock (set)
                        {
                            set.Remove(key);
                        }
                    }
                }
            }
        }

        return Task.CompletedTask;
    }

    public Task InvalidateByTagsAsync(IEnumerable<string> tags, CancellationToken ct)
    {
        foreach (var tag in tags)
        {
            if (_tags.TryRemove(tag, out var set))
            {
                lock (set)
                {
                    foreach (var key in set)
                    {
                        _data.TryRemove(key, out _);
                    }
                }
            }
        }

        return Task.CompletedTask;
    }
}

// ---------- Messages & handlers

public sealed class CallCounter
{
    public int Calls { get; private set; }
    public void Reset()
    {
        Calls = 0;
    }
    public void Inc()
    {
        Calls++;
    }
}

public sealed record AskNow(string Key, bool Bypass = false, bool CacheNull = false) : IAsk<string?>, ICacheable
{
    public string CacheKey => Key;
    public bool BypassCaching => Bypass;
    public bool CacheNulls => CacheNull;
    public TimeSpan? Ttl => null;
    public IReadOnlyCollection<string>? Tags => new[] { $"tag:{Key}" };
}

public sealed class AskNowHandler : IMessageHandler<AskNow, string?>
{
    private readonly CallCounter _counter;

    public AskNowHandler(CallCounter counter)
    {
        _counter = counter;
    }

    public Task<string?> Handle(AskNow message, CancellationToken ct)
    {
        _counter.Inc();
        return Task.FromResult<string?>($"t:{Guid.NewGuid():N}");
    }
}

public sealed record AskNull(string Key, bool CacheNull) : IAsk<string?>, ICacheable
{
    public string CacheKey => Key;
    public bool CacheNulls => CacheNull;
}

public sealed class AskNullHandler : IMessageHandler<AskNull, string?>
{
    private readonly CallCounter _counter;

    public AskNullHandler(CallCounter counter)
    {
        _counter = counter;
    }

    public Task<string?> Handle(AskNull message, CancellationToken ct)
    {
        _counter.Inc();
        return Task.FromResult<string?>(null);
    }
}

public sealed record ActTouch(string Tag, string? FullKey = null) : IAct, ICacheInvalidation
{
    public IReadOnlyCollection<string>? Tags => new[] { Tag };
    public IReadOnlyCollection<string>? Keys => FullKey is null ? null : new[] { FullKey };
}

public sealed class ActTouchHandler : IMessageHandler<ActTouch, Nil>
{
    public Task<Nil> Handle(ActTouch message, CancellationToken ct)
    {
        return Task.FromResult(Nil.Value);
    }
}