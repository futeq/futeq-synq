using FQ.SynQ.Filtering;

namespace FQ.SynQ.Configuration;

public enum Category { Act, Ask }

public sealed class CategoryConfigurator
{
    private readonly FilterCatalogConfig _cfg;
    private readonly Category _cat;

    internal CategoryConfigurator(FilterCatalogConfig cfg, Category cat) { _cfg = cfg; _cat = cat; }

    /// <summary>
    /// Sets a filter for current category.
    /// </summary>
    /// <typeparam name="TFilter">Type of the filter to use</typeparam>
    /// <returns></returns>
    public PipelineConfigurator Use<TFilter>() where TFilter : class
    {
        if (_cat == Category.Act)
        {
            _cfg.ActFilters.Add(typeof(TFilter));
        }
        else
        {
            _cfg.AskFilters.Add(typeof(TFilter));
        }
        
        return new PipelineConfigurator(_cfg);
    }
    
    public PipelineConfigurator Use(Type openGenericFilter)
    {
        (_cat == Category.Act ? _cfg.ActFilters : _cfg.AskFilters).Add(openGenericFilter);
        return new PipelineConfigurator(_cfg);
    }
}