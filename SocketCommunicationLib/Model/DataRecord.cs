namespace SocketCommunicationLib.Model;

public record DataRecord(string Id, LockTime LockTime, string CreatedClientId, int Value)
{
    public static DataRecord Empty => new DataRecord(string.Empty, LockTime.Empty, string.Empty, 0);

    public DataRecord CopyWith(LockTime lockTime, int value)
    {
        return this with { LockTime = lockTime, Value = value };
    }
}