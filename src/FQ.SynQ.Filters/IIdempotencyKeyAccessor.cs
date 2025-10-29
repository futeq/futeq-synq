namespace FQ.SynQ.Filters;

/// <summary>Provides the idempotency key for the current request scope.</summary>
public interface IIdempotencyKeyAccessor
{
    string? GetKey();
}