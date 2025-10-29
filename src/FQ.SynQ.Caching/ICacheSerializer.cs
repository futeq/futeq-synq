namespace FQ.SynQ.Caching;

/// <summary>
/// Defines a serializer used to convert objects to and from cached binary representations.
/// </summary>
public interface ICacheSerializer
{
    /// <summary>
    /// Serializes an object instance into a byte array.
    /// </summary>
    /// <typeparam name="T">The object type.</typeparam>
    /// <param name="value">The object to serialize.</param>
    /// <returns>A UTF-8 encoded binary representation of the object.</returns>
    byte[] Serialize<T>(T value);

    /// <summary>
    /// Deserializes a byte array into a typed object instance.
    /// </summary>
    /// <typeparam name="T">The object type.</typeparam>
    /// <param name="bytes">The serialized payload.</param>
    /// <returns>The deserialized object instance, or <c>null</c> if deserialization fails.</returns>
    T? Deserialize<T>(byte[] bytes);
}