namespace FQ.SynQ;

public interface ISynq
{
    Task<TResponse> Dispatch<TResponse>(IMessage<TResponse> request, CancellationToken ct = default);
}