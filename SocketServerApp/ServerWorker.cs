using System.Net.Sockets;
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
    private readonly QueryDataHandler _queryDataHandler;
    private readonly NextDataHandler _nextDataHandler;
    private readonly DataStore _dataStore;
    private readonly ClientCommunicator _communicator;
    private readonly ServerTerminator _serverTerminator;
    private readonly Socket _clientSocket;
    private readonly AllCilentsCommunicator _allAllCilentsCommunicator;
    private readonly StartStateStore _startStateStore;
    private readonly TimeSpan _initLockTime;

    public ServerWorker(
        string clientId,
        DataStore dataStore,
        ClientCommunicator communicator,
        AllCilentsCommunicator allAllCilentsCommunicator,
        ServerTerminator serverTerminator,
        Socket clientSocket,
        StartStateStore startStateStore,
        TimeSpan initLockTime
        )
    {
        _dataStore = dataStore;
        _communicator = communicator;
        _serverTerminator = serverTerminator;
        _clientSocket = clientSocket;
        _allAllCilentsCommunicator = allAllCilentsCommunicator;
        _startStateStore = startStateStore;
        _initLockTime = initLockTime;
        
        _queryDataHandler = new QueryDataHandler(clientId, communicator, dataStore);
        _nextDataHandler = new NextDataHandler(dataStore, clientId, communicator, allAllCilentsCommunicator);
    }

    public async Task RunAsync(int processorCount, CancellationToken cancellationToken)
    {
        var jobChannel = new ServerJobChannel<Message>();
        var socketListener = CreateSocketListener();
        
        var tasks = new List<Task>();
        var processors = new List<ServerJobProcessor>();

        for (int i = 0; i < processorCount; i++)
        {
            var processor = CreateJobProcessor();
            processors.Add(processor);
            tasks.Add(processor.ProcessAsync(jobChannel, cancellationToken));
        }
        var listenTask = socketListener.ListenAsync(jobChannel, cancellationToken);
        tasks.Add(listenTask);
        
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
        await _allAllCilentsCommunicator.SendNextLockTimeAsync(string.Empty, record, cancellationToken);
    }

    private SocketListener CreateSocketListener()
    {
        return new SocketListener(
            _clientSocket,
            new SocketMessageStringExtractor(
                ProtocolConstants.Eom,
                Encoding.UTF8));
    }

    private ServerJobProcessor CreateJobProcessor()
    {
        return new ServerJobProcessor(
            _dataStore,
            _communicator,
            _queryDataHandler,
            _nextDataHandler,
            _serverTerminator);
    }
}