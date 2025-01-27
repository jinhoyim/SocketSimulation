using System.Net;
using System.Net.Sockets;

namespace SocketServerApp;

public class ConnectionListener : IDisposable
{
    private readonly Socket _socket;
    private readonly LingerOption _lingerOption;

    private ConnectionListener(Socket socket, LingerOption lingerOption)
    {
        _socket = socket;
        _lingerOption = lingerOption;
    }
    
    public static ConnectionListener Create(
        IPEndPoint ipEndPoint,
        LingerOption lingerOption,
        int socketConnectionQueue)
    {
        var socket = new Socket(
            ipEndPoint.AddressFamily,
            SocketType.Stream,
            ProtocolType.Tcp);
        
        socket.Bind(ipEndPoint);
        socket.Listen(socketConnectionQueue);
        return new ConnectionListener(socket, lingerOption);
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