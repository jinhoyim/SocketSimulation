using System.Collections.Concurrent;
using SocketCommunicationLib.Model;
using static SocketCommunicationLib.Contract.ProtocolConstants;

namespace SocketServerApp.Communication;

public class SocketsCommunicator
{
    private readonly ConcurrentDictionary<string,ServerCommunicator> _clients;

    public SocketsCommunicator(ConcurrentDictionary<string, ServerCommunicator> clients)
    {
        _clients = clients;
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