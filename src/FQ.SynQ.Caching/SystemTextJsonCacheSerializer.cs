using System.Text.Json;

namespace FQ.SynQ.Caching;


public sealed class SystemTextJsonCacheSerializer : ICacheSerializer
{
    private readonly JsonSerializerOptions _options;
    
    public SystemTextJsonCacheSerializer(JsonSerializerOptions? options = null)
    {
        _options = options ?? new(JsonSerializerDefaults.Web);
    }

    public byte[] Serialize<T>(T value) => JsonSerializer.SerializeToUtf8Bytes(value, _options);
    public T? Deserialize<T>(byte[] bytes) => JsonSerializer.Deserialize<T>(bytes, _options);
}