namespace SocketCommunicationLib.Contract;

public record Message(string Type, string Content)
{
    public static Message Empty => new Message(string.Empty, string.Empty);
}