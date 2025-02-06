using Microsoft.Extensions.Options;
using SocketServerApp.Communication;

namespace SocketServerApp.Store;

public class StartStateStore
{
    private bool _isStarted = false;
    private readonly Lock _lock = new();
    private readonly int _startConnectionCount;
    private readonly AllCilentsCommunicator _clients;

    public StartStateStore(AllCilentsCommunicator clients, IOptions<ServerConfig> config)
    {
        _clients = clients;
        _startConnectionCount = config.Value.StartConnectionCount;
    }
    
    public bool CanInitAndFirstSend()
    {
        lock (_lock)
        {
            return !_isStarted && _clients.Count >= _startConnectionCount;
        } 
    }

    public void MarkStarted()
    {
        lock (_lock)
        {
            _isStarted = true;
        }
    }
}