namespace FQ.SynQ;

/// <summary>
/// Represents a void type in generic contexts.
/// </summary>
public readonly struct Nil
{
    /// <summary>A singleton instance.</summary>
    public static readonly Nil Value = new();
}