namespace FQ.SynQ.Caching;

/// <summary>
/// Marks a command message that triggers cache invalidation after successful execution.
/// </summary>
public interface ICacheInvalidation
{
    /// <summary>
    /// Gets explicit cache keys to invalidate after the command executes.
    /// </summary>
    IReadOnlyCollection<string>? Keys => null;

    /// <summary>
    /// Gets cache tags to invalidate after the command executes.
    /// </summary>
    IReadOnlyCollection<string>? Tags => null;
}