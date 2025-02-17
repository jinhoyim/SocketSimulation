using System.Net.Sockets;
using SocketServerApp.Communication;

namespace SocketServerApp.Factories;

public class ClientCommunicatorFactory
{
    public ClientCommunicator Create(ClientSession session)
    {
        return new ClientCommunicator(session);
    }
}