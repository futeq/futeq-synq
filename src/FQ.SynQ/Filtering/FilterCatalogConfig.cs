namespace FQ.SynQ.Filtering;

internal sealed class FilterCatalogConfig
{
    public List<Type> GlobalFilters { get; } = new();
    
    public List<Type> ActFilters { get; } = new();
    
    public List<Type> AskFilters { get; } = new();
    
    public Dictionary<Type, List<Type>> PerMessage { get; } = new(); 
    
    public List<(Func<Type, bool> Predicate, List<Type> Filters)> PredicateRules { get; } = new();
}