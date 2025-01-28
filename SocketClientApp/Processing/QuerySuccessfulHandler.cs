using SocketClientApp.Communication;
using SocketClientApp.Output;
using SocketClientApp.Store;
using SocketCommunicationLib.Model;

namespace SocketClientApp.Processing;

public class QuerySuccessfulHandler
{
    private readonly CountStore _countStore;
    private readonly OutputWriter _writer;
    private readonly Random _random;
    private readonly int _maxMilliseconds = 2000;
    private readonly ClientCommunicator _communicator;

    public QuerySuccessfulHandler(
        ClientCommunicator communicator,
        CountStore countStore,
        OutputWriter writer)
    {
        _random = new Random();
        _communicator = communicator;
        _countStore = countStore;
        _writer = writer;
    }

    public async Task SaveAndNextAsync(DataRecordWithNext withNext, CancellationToken cancellationToken)
    {
        var successful = _countStore.IncrementSuccessful();

        _writer.Write(withNext.DataRecord, successful);

        await SendNextDataAsync(cancellationToken, withNext);
    }

    private async Task SendNextDataAsync(CancellationToken cancellationToken, DataRecordWithNext withNext)
    {
        if (withNext.NextId is not null)
        {
            var nextData = CreateNewData(withNext.NextId);
            await _communicator.SendNextDataAsync(nextData, cancellationToken);
        }
    }

    private NextDataValue CreateNewData(string nextId)
    {
        var nextMilliseconds = _random.Next(_maxMilliseconds);
        var nextDateTime = DateTime.Now.AddMilliseconds(nextMilliseconds);
        LockTime lockTime = LockTime.From(nextDateTime);
        return new NextDataValue(nextId, lockTime, lockTime.Millisecond);
    }
}