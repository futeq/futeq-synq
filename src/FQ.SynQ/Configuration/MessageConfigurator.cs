using FQ.SynQ.Filtering;

namespace FQ.SynQ.Configuration;

public sealed class MessageConfigurator
{
    private readonly FilterCatalogConfig _cfg;
    private readonly Type _messageType;

    internal MessageConfigurator(FilterCatalogConfig cfg, Type messageType) { _cfg = cfg; _messageType = messageType; }

    /// <summary>
    /// Sets a filter for current message type.
    /// </summary>
    /// <typeparam name="TFilter">Type of the filter to use</typeparam> 
    public PipelineConfigurator Use<TFilter>() where TFilter : class
    {
        if (!_cfg.PerMessage.TryGetValue(_messageType, out var list))
            _cfg.PerMessage[_messageType] = list = new List<Type>();
        list.Add(typeof(TFilter));
        return new PipelineConfigurator(_cfg);
    }
    
    public PipelineConfigurator Use(Type openGenericFilter)
    {
        if (!_cfg.PerMessage.TryGetValue(_messageType, out var list))
            _cfg.PerMessage[_messageType] = list = new List<Type>();
        list.Add(openGenericFilter);
        return new PipelineConfigurator(_cfg);
    }
}