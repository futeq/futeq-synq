namespace FQ.SynQ.Filters;

/// <summary>Defines a transactional boundary for command handling.</summary>
public interface IUnitOfWork
{
    Task BeginAsync(CancellationToken ct);
    
    Task CommitAsync(CancellationToken ct);
    
    Task RollbackAsync(CancellationToken ct);
}