namespace FQ.SynQ.Filters;

/// <summary>Stores and retrieves serialized handler outcomes for idempotency.</summary>
public interface IIdempotencyStore
{
    Task<(bool, byte[] payload, string contentType)> TryGetAsync(string key, string messageType, CancellationToken ct);
    
    Task SetAsync(string key, string messageType, byte[] payload, string contentType, TimeSpan ttl, CancellationToken ct);
}