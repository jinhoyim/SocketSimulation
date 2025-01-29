using System.Text;
using SocketCommunicationLib;
using SocketCommunicationLib.Contract;
using SocketCommunicationLib.Model;
using SocketServerApp.Channel;
using SocketServerApp.Communication;
using SocketServerApp.Processing;
using SocketServerApp.Store;

namespace SocketServerApp;

public class ServerWorker
{
    private readonly IDataStore _dataStore;
    private readonly ClientCommunicator _client;
    private readonly ServerTerminator _serverTerminator;
    private readonly AllCilentsCommunicator _allClients;
    private readonly StartStateStore _startStateStore;
    private readonly TimeSpan _initLockTime;

    public ServerWorker(
        IDataStore dataStore,
        ClientCommunicator client,
        AllCilentsCommunicator allClients,
        ServerTerminator serverTerminator,
        StartStateStore startStateStore,
        TimeSpan initLockTime
        )
    {
        _dataStore = dataStore;
        _client = client;
        _serverTerminator = serverTerminator;
        _allClients = allClients;
        _startStateStore = startStateStore;
        _initLockTime = initLockTime;
    }

    public async Task RunAsync(int processorCount, CancellationToken cancellationToken)
    {
        var jobChannel = new ServerJobChannel<Message>();
        var socketListener = CreateSocketListener();
        
        var tasks = Enumerable.Range(0, processorCount)
            .Select(t => CreateJobProcessor().ProcessAsync(jobChannel, cancellationToken))
            .ToList();
        tasks.Add(socketListener.ListenAsync(jobChannel, cancellationToken));
        
        await CheckStart(cancellationToken);
        await Task.WhenAll(tasks);
    }

    private async Task CheckStart(CancellationToken cancellationToken)
    {
        if (_startStateStore.CanInitAndFirstSend())
        {
            var initialData = _dataStore.InitialDataRecord(_initLockTime);
            await FirstSendLockTimeAsync(initialData, cancellationToken);
            _startStateStore.MarkStarted();
        }
    }

    private async Task FirstSendLockTimeAsync(DataRecord record, CancellationToken cancellationToken)
    {
        await _allClients.SendNextLockTimeAsync(string.Empty, record, cancellationToken);
    }

    private SocketListener CreateSocketListener()
    {
        return new SocketListener(
            _client.Socket,
            new SocketMessageStringExtractor(
                ProtocolConstants.Eom,
                Encoding.UTF8));
    }

    private ServerJobProcessor CreateJobProcessor()
    {
        return new ServerJobProcessor(
            _dataStore,
            _client,
            new QueryDataHandler(_client.ClientId, _client, _dataStore),
            new NextDataHandler(_dataStore, _client.ClientId, _client, _allClients),
            _serverTerminator);
    }
}