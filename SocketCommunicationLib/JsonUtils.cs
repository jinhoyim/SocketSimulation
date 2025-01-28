using System.Text.Json;

namespace SocketCommunicationLib;

public static class JsonUtils
{
    private static readonly JsonSerializerOptions Options;

    static JsonUtils()
    {
        Options = JsonSerializerOptions.Default;
    }
    
    public static string Serialize<T>(T value) => JsonSerializer.Serialize(value, Options);
    public static T? Deserialize<T>(string json) => JsonSerializer.Deserialize<T>(json, Options);
}