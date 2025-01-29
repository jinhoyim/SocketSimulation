using System.Collections.Concurrent;
using SocketCommunicationLib.Model;
using static SocketCommunicationLib.Contract.ProtocolConstants;

namespace SocketServerApp.Communication;

public class AllCilentsCommunicator
{
    private readonly ConcurrentDictionary<string, ClientCommunicator> _clients = new();

    public int Count => _clients.Count;
    public ICollection<string> ClientIds => _clients.Keys;
    public bool IsEmpty => _clients.IsEmpty;
    
    public void Add(string clientId, ClientCommunicator communicator)
    {
        _clients[clientId] = communicator;
    }

    public void TryRemove(string clientId)
    {
        _clients.TryRemove(clientId, out _);
    }

    public async Task SendServerTerminateAsync(CancellationToken cancellationToken)
    {
        var tasks = _clients.Values.Select(client =>
            client.SendStringAsync("", ServerTerminated, cancellationToken));
        await Task.WhenAll(tasks);
    }

    public async Task SendNextLockTimeAsync(string clientId, DataRecord record, CancellationToken cancellationToken)
    {
        var tasks = _clients.Where(c => c.Key != clientId)
            .Select(s => s.Value.SendLockTimeAsync(record, cancellationToken));
        await Task.WhenAll(tasks);
    }
}