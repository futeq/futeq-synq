namespace FQ.SynQ; 

/// <summary>Marker for a message handled by exactly one handler.</summary>
public interface IMessage<out TOut> { }