namespace FQ.SynQ;

public interface IFilterCatalog
{
    IEnumerable<object> CreateFilters(Type messageType, Type outType, IServiceProvider sp);
}