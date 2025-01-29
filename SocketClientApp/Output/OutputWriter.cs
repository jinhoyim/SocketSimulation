using SocketClientApp.Store;
using SocketCommunicationLib.Model;

namespace SocketClientApp.Output;

public class OutputWriter
{
    private readonly CountStore _store;

    public OutputWriter(CountStore store)
    {
        _store = store;
    }
    
    public void WriteSuccess(DataRecord record)
    {
        var (id, lockTime, createdClientId, value) = record;
        Console.WriteLine($"[{_store}] Success: Id: {id}, LockTime: {lockTime}, Created: {createdClientId}, Value: {value}");
    }

    public void WriteError(string message)
    {
        Console.WriteLine($"[{_store}] Error: {message}");
    }
}