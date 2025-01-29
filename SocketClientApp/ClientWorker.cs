using System.Net.Sockets;
using System.Text;
using SocketClientApp.Channel;
using SocketClientApp.Communication;
using SocketClientApp.Output;
using SocketClientApp.Processing;
using SocketClientApp.Store;
using SocketCommunicationLib;
using SocketCommunicationLib.Contract;

namespace SocketClientApp;

public class ClientWorker
{
    private readonly ClientCommunicator _communicator;
    private readonly LockTimesStore _lockTimesStore;
    private readonly CountStore _countStore;
    private readonly OutputWriter _writer;
    private readonly int _maxMilliseconds;
    private readonly CancellationTokenSource _cts;
    private readonly bool _afterLockTime;

    public ClientWorker(Socket serverSocket, int maxMilliseconds, bool afterLockTime, CancellationTokenSource cts)
    {
        _maxMilliseconds = maxMilliseconds;
        _cts = cts;
        _communicator = new ClientCommunicator(serverSocket);
        _lockTimesStore = new LockTimesStore();
        _countStore = new CountStore();
        _writer = new OutputWriter(_countStore);
        _afterLockTime = afterLockTime;
    }

    public async Task RunAsync(int processorCount, CancellationToken cancellationToken)
    {
        var jobChannel = new ClientJobChannel<Message>();        
        var messageListener = CreateSocketListener();

        var tasks = Enumerable.Range(0, processorCount)
            .Select(t => CreateJobProcessor().ProcessAsync(jobChannel, cancellationToken))
            .ToList();
        tasks.Add(messageListener.ListenAsync(jobChannel, cancellationToken));
        
        await Task.WhenAll(tasks);
    }

    private ClientJobProcessor CreateJobProcessor()
    {
        return new ClientJobProcessor(
            new QuerySuccessfulHandler(
                _communicator,
                new NextDataGenerator(_maxMilliseconds),
                _countStore,
                _writer),
            new QueryHandler(_communicator, _lockTimesStore, _afterLockTime),
            new ErrorHandler(_countStore, _writer, _lockTimesStore),
            _cts);
    }

    private SocketListener CreateSocketListener()
    {
        return new SocketListener(
            _communicator.Socket,
            new SocketMessageStringExtractor(
                ProtocolConstants.Eom,
                Encoding.UTF8)
        );
    }
}