using System.Net.Sockets;
using Microsoft.Extensions.Options;

namespace SocketServerApp;

public class ServerListener : IDisposable
{
    private readonly Socket _socket;
    private readonly LingerOption _lingerOption;

    public ServerListener(IOptions<ServerConfig> config)
    {
        var ipEndPoint = config.Value.IpEndPoint;
        _socket = new Socket(
            ipEndPoint.AddressFamily,
            SocketType.Stream,
            ProtocolType.Tcp);
        _socket.Bind(ipEndPoint);
        _socket.Listen(config.Value.SocketConnectionQueue);
        _lingerOption = config.Value.LingerOption;
    }

    public async Task<Socket> AcceptAsync(CancellationToken cancellationToken = default)
    {
        var clientSocket = await _socket.AcceptAsync(cancellationToken);
        clientSocket.LingerState = _lingerOption;
        return clientSocket;
    }

    public void Dispose()
    {
        if (_socket.Connected)
        {
            _socket.Shutdown(SocketShutdown.Both);
        }
        _socket.Dispose();
    }
}