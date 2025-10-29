using Microsoft.Extensions.DependencyInjection;

namespace FQ.SynQ;

public static class ServiceCollectionExtensions
{
    public static SynqBuilder AddSynq(this IServiceCollection services)
        => new SynqBuilder(services);

    public static IServiceCollection AddSynq(this IServiceCollection services, Action<SynqBuilder> configure)
    {
        var b = new SynqBuilder(services);
        configure(b);
        b.Build();
        return services;
    }
}