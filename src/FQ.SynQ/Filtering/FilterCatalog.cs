using Microsoft.Extensions.DependencyInjection;

namespace FQ.SynQ.Filtering;

internal sealed class FilterCatalog : IFilterCatalog
{
    private readonly FilterCatalogConfig _cfg;

    public FilterCatalog(FilterCatalogConfig cfg) => _cfg = cfg;

    public IEnumerable<object> CreateFilters(Type messageType, Type outType, IServiceProvider sp)
    { 
        object Instantiate(Type openFilter)
        {
            var closed = openFilter.MakeGenericType(messageType, outType);
            return ActivatorUtilities.CreateInstance(sp, closed);
        }
 
        foreach (var f in _cfg.GlobalFilters)
        {
            yield return Instantiate(f);
        }

        var isAct = typeof(IAct).IsAssignableFrom(messageType) ||
                    messageType.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IAct<>));
        var isAsk = messageType.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IAsk<>));

        if (isAct)
        {
            foreach (var f in _cfg.ActFilters) yield return Instantiate(f);
        }

        if (isAsk)
        {
            foreach (var f in _cfg.AskFilters) yield return Instantiate(f);
        }

        foreach (var (pred, filters) in _cfg.PredicateRules)
        {
            if (pred(messageType))
            {
                foreach (var f in filters) yield return Instantiate(f);
            }
        }

        if (_cfg.PerMessage.TryGetValue(messageType, out var list))
        {
            foreach (var f in list)
            {
                yield return Instantiate(f);
            }
        }
    }
}