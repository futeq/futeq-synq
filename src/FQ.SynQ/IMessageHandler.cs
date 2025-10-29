namespace FQ.SynQ;  

/// <summary>
/// Defines the contract for handling a specific <typeparamref name="TRequest"/> and producing a <typeparamref name="TResponse"/>.
/// </summary>
/// <typeparam name="TRequest">The request type.</typeparam>
/// <typeparam name="TResponse">The response type.</typeparam>
public interface IMessageHandler<in TRequest, TResponse>
    where TRequest : IMessage<TResponse>
{
    Task<TResponse> Handle(TRequest request, CancellationToken ct);
}