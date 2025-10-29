using FQ.SynQ.Filtering;

namespace FQ.SynQ.Configuration;


public sealed class PredicateConfigurator
{
    private readonly FilterCatalogConfig _cfg;
    private readonly Func<Type, bool> _predicate;

    internal PredicateConfigurator(FilterCatalogConfig cfg, Func<Type, bool> predicate)
    {
        _cfg = cfg; _predicate = predicate;
    }

    /// <summary>
    /// Sets a filter for current predicate.
    /// </summary>
    /// <typeparam name="TFilter">Type of the filter to use</typeparam>
    /// <returns></returns>
    public PipelineConfigurator Use<TFilter>() where TFilter : class
    {
        _cfg.PredicateRules.Add((_predicate, new List<Type> { typeof(TFilter) }));
        return new PipelineConfigurator(_cfg);
    }
    
    public PipelineConfigurator Use(Type openGenericFilter)
    {
        _cfg.PredicateRules.Add((_predicate, new List<Type> { openGenericFilter }));
        return new PipelineConfigurator(_cfg);
    }
}