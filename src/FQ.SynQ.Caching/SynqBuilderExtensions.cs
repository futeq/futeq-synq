using Microsoft.Extensions.DependencyInjection;

namespace FQ.SynQ.Caching;

public static class SynqBuilderExtensions
{
    /// <summary>
    /// Adds read-side caching for queries (IAsk&lt;T&gt;) with safe default ordering:
    /// after Validation/Authorization, before handler.
    /// </summary>
    public static SynqBuilder UseCaching(this SynqBuilder builder)
        => builder.Pipelines(p => p.ForAsks().Use(typeof(CacheFilter<,>)) );

    /// <summary>
    /// Adds write-side cache invalidation for commands (IAct / IAct&lt;T&gt;) as the outermost filter
    /// (added last) so it runs after UoW & DomainEvents.
    /// </summary>
    public static SynqBuilder UseCacheInvalidation(this SynqBuilder builder)
        => builder.Pipelines(p => p.ForActs().Use(typeof(CacheInvalidationFilter<,>)) );

    /// <summary>
    /// Convenience preset: query caching + command invalidation.
    /// </summary>
    public static SynqBuilder AddCachingPreset(this SynqBuilder builder, Action<AskCacheOptions>? configure = null)
    {
        // allow caller to tweak options
        builder.Pipelines(_ => { }); // no-op to ensure builder exists
        builder.Services().AddSynqCaching(configure);
        return builder
            .UseCaching()
            .UseCacheInvalidation();
    }

    private static IServiceCollection Services(this SynqBuilder builder)
        => (IServiceCollection)builder.GetType()
            .GetField("_services", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
            .GetValue(builder)!;
}