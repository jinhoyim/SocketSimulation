namespace SocketCommunicationLib.Contract;

public record DataRecord(string Id, LockTime LockTime, string CreatedClientId, int Value)
{
    public static DataRecord Empty => new DataRecord(string.Empty, LockTime.Empty, string.Empty, 0);
}