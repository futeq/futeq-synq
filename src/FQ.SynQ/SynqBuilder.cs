using System.Reflection;
using FQ.SynQ.Configuration;
using FQ.SynQ.Dispatchers;
using FQ.SynQ.Filtering;
using Microsoft.Extensions.DependencyInjection;

namespace FQ.SynQ;

public sealed class SynqBuilder
{
    private readonly IServiceCollection _services;
    private readonly FilterCatalogConfig _cfg = new();

    internal SynqBuilder(IServiceCollection services)
    {
        _services = services;
    }

    public SynqBuilder ScanHandlers(params Assembly[] assemblies)
    {
        foreach (var asm in assemblies)
        {
            var handlers = asm.GetTypes()
                .Where(t => !t.IsAbstract && !t.IsInterface)
                .SelectMany(t => t.GetInterfaces()
                    .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IMessageHandler<,>))
                    .Select(i => new { Service = i, Impl = t }));

            foreach (var h in handlers)
                _services.AddTransient(h.Service, h.Impl);
        }
        return this;
    }

    public SynqBuilder Pipelines(Action<PipelineConfigurator> configure)
    {
        var pc = new PipelineConfigurator(_cfg);
        
        configure(pc);
        
        return this;
    }

    internal void Build()
    {
        _services.AddSingleton<IFilterCatalog>(new FilterCatalog(_cfg));
        _services.AddSingleton<ISynq, SynqDispatcher>();
    }
}