namespace FQ.SynQ.Caching;

/// <summary>
/// Provides partition context for cache key generation,
/// such as tenant, region, or user identifiers.
/// </summary>
public interface ICachePartitionAccessor
{
    /// <summary>
    /// Gets the current cache partition key, or <c>null</c> if no partition applies.
    /// </summary>
    /// <returns>A string representing the partition, or <c>null</c>.</returns>
    string? GetPartition();
}