namespace SocketCommunicationLib.Contract;

public record Message(string Type, object Content)
{
    public static Message Empty => new Message(string.Empty, string.Empty);
}