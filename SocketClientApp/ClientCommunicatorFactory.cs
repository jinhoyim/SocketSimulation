using System.Net.Sockets;
using SocketClientApp.Communication;

namespace SocketClientApp;

public class ClientCommunicatorFactory
{
    public ClientCommunicator Create(Socket socket)
    {
        return new ClientCommunicator(socket);
    }
}