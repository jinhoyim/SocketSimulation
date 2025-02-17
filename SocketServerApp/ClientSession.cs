using System.Net.Sockets;

namespace SocketServerApp;

public class ClientSession : IDisposable
{
    public string ClientId { get; }
    public Socket Socket { get; }

    public ClientSession(string clientId, Socket socket)
    {
        ClientId = clientId;
        Socket = socket;
    }

    public void Dispose()
    {
        if (Socket.Connected)
        {
            Socket.Shutdown(SocketShutdown.Both);
        }
        Socket.Dispose();
    }
}