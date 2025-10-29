using Microsoft.Extensions.DependencyInjection;

namespace FQ.SynQ.Caching;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers core caching services. You must also register an <see cref="ICacheStore"/> implementation.
    /// </summary>
    public static IServiceCollection AddSynqCaching(this IServiceCollection services, Action<AskCacheOptions>? configure = null)
    {
        services.AddSingleton<ICacheSerializer, SystemTextJsonCacheSerializer>();

        if (configure is not null)
        {
            services.Configure(configure);
        }
        else 
        {
            services.AddOptions<AskCacheOptions>(); // defaults
        }
        
        return services;
    }
}