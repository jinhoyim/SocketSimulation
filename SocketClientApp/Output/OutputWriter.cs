using SocketCommunicationLib.Contract;

namespace SocketClientApp.Output;

public class OutputWriter
{
    public void Write(DataRecord record, int successful)
    {
        var (id, lockTime, createdClientId, value) = record;
        Console.WriteLine($"Success: {successful}, Id: {id}, LockTime: {lockTime}, Created: {createdClientId}, Value: {value}");
    }
}