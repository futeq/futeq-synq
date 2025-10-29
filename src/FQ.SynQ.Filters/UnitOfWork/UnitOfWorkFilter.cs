namespace FQ.SynQ.Filters.UnitOfWork; 

/// <summary>Wraps command execution in a unit-of-work (transaction) boundary.</summary>
public sealed class UnitOfWorkFilter<T, TOut> : IFilter<T, TOut>
    where T : IMessage<TOut>
{
    private readonly IUnitOfWork _uow;

    public UnitOfWorkFilter(IUnitOfWork uow) => _uow = uow;

    public async Task<TOut> Invoke(T message, CancellationToken ct, Next<TOut> next)
    {
        var isAct = typeof(IAct).IsAssignableFrom(typeof(T)) ||
                    typeof(T).GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IAct<>));

        if (!isAct)
        {
            return await next(ct);
        }

        await _uow.BeginAsync(ct);
        
        try
        {
            var result = await next(ct);
            await _uow.CommitAsync(ct);
            return result;
        }
        catch
        {
            try
            {
                await _uow.RollbackAsync(ct);
            } catch 
            { /* swallow rollback errors */ }
            throw;
        }
    }
}