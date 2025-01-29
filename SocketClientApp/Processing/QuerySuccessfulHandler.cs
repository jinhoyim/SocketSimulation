using SocketClientApp.Communication;
using SocketClientApp.Output;
using SocketClientApp.Store;
using SocketCommunicationLib.Model;

namespace SocketClientApp.Processing;

public class QuerySuccessfulHandler
{
    private readonly ClientCommunicator _communicator;
    private readonly NextDataGenerator _nextDataGenerator;
    private readonly CountStore _countStore;
    private readonly OutputWriter _writer;

    public QuerySuccessfulHandler(
        ClientCommunicator communicator,
        NextDataGenerator nextDataGenerator,
        CountStore countStore,
        OutputWriter writer)
    {
        _communicator = communicator;
        _nextDataGenerator = nextDataGenerator;
        _countStore = countStore;
        _writer = writer;
    }

    public async Task SaveAndNextAsync(DataRecordWithNext withNext, CancellationToken cancellationToken)
    {
        _countStore.IncrementSuccessful();
        _writer.WriteSuccess(withNext.DataRecord);
        await  SendNextDataAsync(cancellationToken, withNext);
    }

    private async Task SendNextDataAsync(CancellationToken cancellationToken, DataRecordWithNext withNext)
    {
        if (withNext.NextId is not null)
        {
            var nextData = _nextDataGenerator.CreateNewData(withNext.NextId);
            await _communicator.SendNextDataAsync(nextData, cancellationToken);
        }
    }
}