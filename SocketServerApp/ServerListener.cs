using System.Net.Sockets;
using Microsoft.Extensions.Options;
using SocketServerApp.Factories;

namespace SocketServerApp;

public class ServerListener : IDisposable
{
    private readonly Socket _socket;
    private readonly LingerOption _lingerOption;
    private readonly ClientIdentifierFactory _clientIdentifierFactory;

    public ServerListener(
        ClientIdentifierFactory clientIdentifierFactory,
        IOptions<ServerConfig> config)
    {
        _clientIdentifierFactory = clientIdentifierFactory;
        var ipEndPoint = config.Value.IpEndPoint;
        _socket = new Socket(
            ipEndPoint.AddressFamily,
            SocketType.Stream,
            ProtocolType.Tcp);
        _socket.Bind(ipEndPoint);
        _socket.Listen(config.Value.SocketConnectionQueue);
        _lingerOption = config.Value.LingerOption;
    }

    public async Task<ClientSession?> AcceptAsync(CancellationToken cancellationToken = default)
    {
        var clientSocket = await _socket.AcceptAsync(cancellationToken);
        clientSocket.LingerState = _lingerOption;
        
        var identifier = _clientIdentifierFactory.Create(clientSocket);
        try
        {
            var (verified, clientId) = await identifier.IdentifyClientAsync(cancellationToken);
            if (verified)
            {
                return new ClientSession(clientId, clientSocket);
            }
        }
        catch (Exception ex)
        {
            if (clientSocket.Connected)
            {
                clientSocket.Shutdown(SocketShutdown.Both);
            }
            clientSocket.Dispose();
            Console.WriteLine($"Client Identifier error: {ex.Message}");
        }
        return null;
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