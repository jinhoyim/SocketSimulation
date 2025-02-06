namespace SocketCommunicationLib.Contract;

public record Message(string Type, object Content)
{
    public static Message Empty { get; } = new Message(string.Empty, string.Empty);
}