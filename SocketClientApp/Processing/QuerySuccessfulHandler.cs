using SocketClientApp.Communication;
using SocketClientApp.Output;
using SocketClientApp.Store;
using SocketCommunicationLib;
using SocketCommunicationLib.Contract;

namespace SocketClientApp.Processing;

public class QuerySuccessfulHandler
{
    private readonly DataStore _store;
    private readonly OutputWriter _writer;
    private readonly Random _random;
    private readonly int _maxMilliseconds = 2000;
    private readonly SocketCommunicator _communicator;

    public QuerySuccessfulHandler(
        SocketCommunicator communicator,
        DataStore store,
        OutputWriter writer)
    {
        _random = new Random();
        _communicator = communicator;
        _store = store;
        _writer = writer;
    }

    public async Task SaveAndNextAsync(string content, CancellationToken cancellationToken)
    {
        var withNext = JsonUtils.Deserialize<DataRecordWithNext>(content);
        if (withNext is null) return;

        var successful = _store.IncrementSuccessful();

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