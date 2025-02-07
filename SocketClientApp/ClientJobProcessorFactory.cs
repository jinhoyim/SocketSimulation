using Microsoft.Extensions.Options;
using SocketClientApp.Communication;
using SocketClientApp.Output;
using SocketClientApp.Processing;
using SocketClientApp.Store;

namespace SocketClientApp;

public class ClientJobProcessorFactory
{
    private readonly CountStore _countStore;
    private readonly LockTimesStore _lockTimesStore;
    private readonly OutputWriter _writer;
    private readonly bool _afterLockTime;
    private readonly NextDataGenerator _nextDataGenerator;
    private readonly CancellationTokenSource _cts;

    public ClientJobProcessorFactory(
        IOptions<ClientConfig> config,
        CountStore countStore,
        LockTimesStore lockTimesStore,
        OutputWriter writer,
        NextDataGenerator nextDataGenerator,
        CancellationTokenSource cts)
    {
        _countStore = countStore;
        _lockTimesStore = lockTimesStore;
        _writer = writer;
        _afterLockTime = config.Value.AfterLockTime;
        _nextDataGenerator = nextDataGenerator;
        _cts = cts;
    }

    public ClientJobProcessor Create(ClientCommunicator communicator)
    {
        return new ClientJobProcessor(
            new QuerySuccessfulHandler(
                communicator,
                _nextDataGenerator,
                _countStore,
                _writer),
            new QueryHandler(communicator, _lockTimesStore, _afterLockTime),
            new ErrorHandler(_countStore, _writer, _lockTimesStore),
            _cts);
    }
}