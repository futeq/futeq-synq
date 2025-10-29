using FluentValidation;

namespace FQ.SynQ.Filters.UnitTests;

public sealed record AskPing(string Payload) : IAsk<string>;
public sealed record ActAdd(int By) : IAct<int>;
public sealed record ActBoom() : IAct;

public sealed class AskPingHandler : IMessageHandler<AskPing, string>
{
    public Task<string> Handle(AskPing message, CancellationToken ct)
        => Task.FromResult($"pong:{message.Payload}");
}

public sealed class ActAddHandler : IMessageHandler<ActAdd, int>
{
    private static int _state;
    public static void Reset() => _state = 0;

    public Task<int> Handle(ActAdd message, CancellationToken ct)
    {
        _state += message.By;
        return Task.FromResult(_state);
    }
}

public sealed class ActBoomHandler : IMessageHandler<ActBoom, Nil>
{
    public Task<Nil> Handle(ActBoom message, CancellationToken ct)
        => throw new InvalidOperationException("kaboom");
}

// ---------- Validation ----------

public sealed class AskPingValidator : AbstractValidator<AskPing>
{
    public AskPingValidator()
    {
        RuleFor(x => x.Payload).NotEmpty().MinimumLength(2);
    }
}

// ---------- Authorization ----------

public sealed class DenyAllGuard<T> : IGuard<T>
{
    public Task CheckAsync(T message, CancellationToken ct)
        => throw new UnauthorizedAccessException("nope");
}

public sealed class AllowAllGuard<T> : IGuard<T>
{
    public Task CheckAsync(T message, CancellationToken ct) => Task.CompletedTask;
}

// ---------- Unit of Work ----------

public sealed class FakeUnitOfWork : IUnitOfWork
{
    public List<string> Calls { get; } = new();

    public Task BeginAsync(CancellationToken ct) { Calls.Add("begin"); return Task.CompletedTask; }
    public Task CommitAsync(CancellationToken ct) { Calls.Add("commit"); return Task.CompletedTask; }
    public Task RollbackAsync(CancellationToken ct) { Calls.Add("rollback"); return Task.CompletedTask; }
}

// ---------- Domain Events ----------

public sealed class FakeDomainEventsPublisher : IDomainEventsPublisher
{
    public int Published { get; private set; }
    public Task PublishPendingAsync(CancellationToken ct) { Published++; return Task.CompletedTask; }
}

// ---------- Idempotency ----------

public sealed class FixedIdempotencyKeyAccessor : IIdempotencyKeyAccessor
{
    private readonly string? _key;
    public FixedIdempotencyKeyAccessor(string? key) { _key = key; }
    public string? GetKey() => _key;
}

public sealed class MemoryIdempotencyStore : IIdempotencyStore
{
    private readonly Dictionary<(string key, string type), (byte[] payload, string contentType, DateTimeOffset expires)> _store = new();

    public Task<(bool, byte[] payload, string contentType)> TryGetAsync(string key, string messageType,
        CancellationToken ct)
    {
        if (_store.TryGetValue((key, messageType), out var v) && v.expires > DateTimeOffset.UtcNow)
        {
            return Task.FromResult((true, v.payload, v.contentType));
        }
        
        return Task.FromResult((false, Array.Empty<byte>(), string.Empty));
    }

    public Task SetAsync(string key, string messageType, byte[] payload, string contentType, TimeSpan ttl, CancellationToken ct)
    {
        _store[(key, messageType)] = (payload, contentType, DateTimeOffset.UtcNow.Add(ttl));
        
        return Task.CompletedTask;
    }
}