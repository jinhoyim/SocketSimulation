using System.Collections.Concurrent;
using static SocketCommunicationLib.Contract.ProtocolConstants;

namespace SocketServerApp.Communication;

public class SocketsCommunicator
{
    private readonly ConcurrentDictionary<string,ServerCommunicator> _clients;

    public SocketsCommunicator(ConcurrentDictionary<string, ServerCommunicator> clients)
    {
        _clients = clients;
    }

    public bool TryGetClient(string clientId, out ServerCommunicator? client)
    {
        return _clients.TryGetValue(clientId, out client);
    }
    
    public async Task SendServerTerminateAsync(CancellationToken cancellationToken)
    {
        var tasks = _clients.Values.Select(client =>
            client.SendStringAsync("", ServerTerminated, cancellationToken));
        await Task.WhenAll(tasks);
    }
}