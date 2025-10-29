using Microsoft.Extensions.DependencyInjection;

namespace FQ.SynQ.Caching.UnitTests;

internal static class TestHost
{
    public static ServiceProvider Build(Action<SynqBuilder> configureSynq, Action<IServiceCollection>? more = null)
    {
        var services = new ServiceCollection();
        services.AddLogging();

        services.AddSingleton<IClock, FakeClock>();
        services.AddSingleton<ICacheStore, InMemoryCacheStore>();
        services.AddSingleton<ICachePartitionAccessor, TestPartitionAccessor>();
        services.AddSingleton<ICacheSerializer, SystemTextJsonCacheSerializer>();
        services.AddSynqCaching(opt =>
        {
            opt.DefaultTtl = TimeSpan.FromMinutes(5);
            opt.StampedeProtection = true;
            opt.GlobalBypass = _ => false;
        });

        services.AddSingleton<CallCounter>();

        more?.Invoke(services);

        services.AddSynq(configureSynq);

        return services.BuildServiceProvider();
    }

    public static string BuildFullQueryKey<T>(string userKey, string? partition = null)
    {
        var type = typeof(T);
        if (partition is null)
        {
            return $"synq:q:{type.FullName}|{userKey}";
        }
        else
        {
            return $"synq:q:{partition}|{type.FullName}|{userKey}";
        }
    }
}