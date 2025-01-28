using System.Text.Json;

namespace SocketCommunicationLib;

public static class JsonUtils
{
    public static T? Deserialize<T>(string json) => JsonSerializer.Deserialize<T>(json);
}