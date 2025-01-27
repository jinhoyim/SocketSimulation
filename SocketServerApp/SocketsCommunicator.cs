using System.Collections.Concurrent;
using static SocketCommunicationLib.ProtocolConstants;

namespace SocketServerApp;

public class SocketsCommunicator
{
    private readonly ConcurrentDictionary<string,SocketCommunicator> _clients;

    public SocketsCommunicator(ConcurrentDictionary<string, SocketCommunicator> clients)
    {
        _clients = clients;
    }

    public async Task SendServerTerminateAsync(CancellationToken cancellationToken)
    {
        var tasks = _clients.Values.Select(client =>
        {
            var message = $"{ServerTerminated}{Eom}";
            Console.WriteLine(message);
            return client.SendStringAsync(message, cancellationToken);
        });
        await Task.WhenAll(tasks);
    }
}