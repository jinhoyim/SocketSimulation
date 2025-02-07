using System.Net.Sockets;
using SocketServerApp.Communication;

namespace SocketServerApp.Factories;

public class ClientCommunicatorFactory
{
    public ClientCommunicator Create(string clientId, Socket socket)
    {
        return new ClientCommunicator(clientId, socket);
    }
}