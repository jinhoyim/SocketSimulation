using SocketCommunicationLib.Model;

namespace SocketServerApp.Output;

public class OutputWriter
{
    public void WriteRecordUpdated(DataRecord record)
    {
        Console.WriteLine($"Record updated: {record}");
    }

    public void WriteRecordInit(DataRecord record)
    {
        Console.WriteLine($"Record initialized: {record}");
    }

    public void WriteRecordNextCreated(DataRecord record)
    {
        var (id, _, createdClientId, _) = record;
        Console.WriteLine($"Next record created: Id: {id}, CreatedClientId: {createdClientId}");
    }
}