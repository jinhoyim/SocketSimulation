using Microsoft.Extensions.Options;
using SocketServerApp.Communication;
using SocketServerApp.Processing;
using SocketServerApp.Store;

namespace SocketServerApp.Factories;

public class ServerWorkerFactory
{
    private readonly IDataStore _dataStore;
    private readonly AllCilentsCommunicator _clients;
    private readonly ServerTerminator _serverTerminator;
    private readonly StartStateStore _startStateStore;
    private readonly TimeSpan _initLockTime;
    
    public ServerWorkerFactory(
        IDataStore dataStore,
        AllCilentsCommunicator clients,
        ServerTerminator serverTerminator,
        StartStateStore startStateStore,
        IOptions<ServerConfig> config)
    {
        _dataStore = dataStore;
        _clients = clients;
        _serverTerminator = serverTerminator;
        _startStateStore = startStateStore;
        _initLockTime = config.Value.InitLockTime;
    }

    public ServerWorker Create(ClientCommunicator communicator)
    {
        return new ServerWorker(
            _dataStore,
            communicator,
            _clients,
            _serverTerminator,
            _startStateStore,
            _initLockTime
        );
    }
}