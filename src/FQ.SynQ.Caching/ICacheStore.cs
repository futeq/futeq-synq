namespace FQ.SynQ.Caching;

/// <summary>
/// Represents a low-level cache storage provider used by Synq caching filters.
/// Implementations may target memory, distributed, or hybrid stores.
/// </summary>
public interface ICacheStore
{
    /// <summary>
    /// Attempts to retrieve a cached entry by key.
    /// </summary>
    /// <param name="key">The full cache key.</param>
    /// <param name="ct">A cancellation token.</param>
    /// <returns>
    /// A tuple indicating whether a value was found and, if so, containing the serialized payload and its content type.
    /// </returns>
    Task<(bool, byte[], string Empty)> TryGetAsync(string key, CancellationToken ct);

    /// <summary>
    /// Stores a serialized payload in the cache using the specified key and time-to-live.
    /// </summary>
    /// <param name="key">The cache key.</param>
    /// <param name="payload">The serialized payload bytes.</param>
    /// <param name="contentType">The content type of the payload.</param>
    /// <param name="ttl">The time-to-live duration.</param>
    /// <param name="tags">Optional tags associated with the entry for later invalidation.</param>
    /// <param name="ct">A cancellation token.</param>
    Task SetAsync(string key, byte[] payload, string contentType, TimeSpan ttl, IReadOnlyCollection<string>? tags,
        CancellationToken ct);

    /// <summary>
    /// Invalidates all cached entries matching the specified keys.
    /// </summary>
    /// <param name="keys">The keys to invalidate.</param>
    /// <param name="ct">A cancellation token.</param>
    Task InvalidateByKeysAsync(IEnumerable<string> keys, CancellationToken ct);

    /// <summary>
    /// Invalidates all cached entries matching the specified tags.
    /// </summary>
    /// <param name="tags">The tags to invalidate.</param>
    /// <param name="ct">A cancellation token.</param>
    Task InvalidateByTagsAsync(IEnumerable<string> tags, CancellationToken ct);
}