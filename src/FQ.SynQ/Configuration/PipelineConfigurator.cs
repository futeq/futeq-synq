using FQ.SynQ.Filtering;

namespace FQ.SynQ.Configuration;

public sealed class PipelineConfigurator
{
    private readonly FilterCatalogConfig _cfg;
    internal PipelineConfigurator(FilterCatalogConfig cfg)
    {
        _cfg = cfg;
    }

    /// <summary>Add a global filter for all messages.</summary>
    public PipelineConfigurator Use<TFilter>() where TFilter : class
    {
        _cfg.GlobalFilters.Add(typeof(TFilter));
        return this;
    }

    public PipelineConfigurator Use(Type openGenericFilter)
    {
        _cfg.GlobalFilters.Add(openGenericFilter); return this;
    }

    /// <summary>Filters applied only to commands (IAct / IAct&lt;T&gt;).</summary>
    public CategoryConfigurator ForActs() => new(_cfg, Category.Act);

    /// <summary>Filters applied only to queries (IAsk&lt;T&gt;).</summary>
    public CategoryConfigurator ForAsks() => new(_cfg, Category.Ask);

    /// <summary>Filters applied only to a specific message type.</summary>
    public MessageConfigurator ForMessage<TMessage>() where TMessage : class
        => new(_cfg, typeof(TMessage));

    /// <summary>Filters applied when the predicate matches the message type.</summary>
    public PredicateConfigurator For(Func<Type, bool> predicate)
        => new(_cfg, predicate);
}