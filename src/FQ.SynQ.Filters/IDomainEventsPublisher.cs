namespace FQ.SynQ.Filters;

/// <summary>Publishes domain events collected during command processing.</summary>
public interface IDomainEventsPublisher
{
    Task PublishPendingAsync(CancellationToken ct);
}