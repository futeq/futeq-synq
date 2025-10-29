namespace FQ.SynQ.Caching;

/// <summary>
/// Marks a query message that can be cached by the Synq query caching filter.
/// </summary>
public interface ICacheable
{
    /// <summary>
    /// Gets the logical key identifying this cached result.
    /// </summary>
    string CacheKey { get; }

    /// <summary>
    /// Gets an optional time-to-live for this specific entry.
    /// </summary>
    TimeSpan? Ttl => null;

    /// <summary>
    /// Gets a collection of tags associated with this entry for grouped invalidation.
    /// </summary>
    IReadOnlyCollection<string>? Tags => null;

    /// <summary>
    /// Indicates whether null results should also be cached.
    /// </summary>
    bool CacheNulls => false;

    /// <summary>
    /// Indicates whether caching should be bypassed for this request instance.
    /// </summary>
    bool BypassCaching => false;
}
