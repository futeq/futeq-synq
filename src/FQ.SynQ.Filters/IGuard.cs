namespace FQ.SynQ.Filters;

/// <summary>Authorization guard for a message; throw to deny.</summary>
public interface IGuard<in T>
{
    Task CheckAsync(T message, CancellationToken ct);
}