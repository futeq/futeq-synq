namespace FQ.SynQ.Caching;

/// <summary>
/// Represents global configuration options for query caching behavior.
/// </summary>
public sealed class AskCacheOptions
{
    /// <summary>
    /// Gets or sets the default time-to-live for cached entries when none is specified on the message.
    /// </summary>
    public TimeSpan DefaultTtl { get; set; } = TimeSpan.FromMinutes(5);

    /// <summary>
    /// Gets or sets the default content type used when serializing cached payloads.
    /// </summary>
    public string ContentType { get; set; } = "application/json";

    /// <summary>
    /// Gets or sets a value indicating whether stampede protection is enabled.
    /// </summary>
    public bool StampedeProtection { get; set; } = true;

    /// <summary>
    /// Gets or sets an optional predicate used to further restrict which queries are cached.
    /// </summary>
    public Func<Type, bool>? AdditionalAskPredicate { get; set; }

    /// <summary>
    /// Gets or sets an optional global bypass condition evaluated per message instance.
    /// </summary>
    public Func<object, bool>? GlobalBypass { get; set; }
}
