using System.Net.Sockets;
using SocketServerApp.Communication;

namespace SocketServerApp.Factories;

public class ClientIdentifierFactory
{
    private readonly AllCilentsCommunicator _clients;

    public ClientIdentifierFactory(AllCilentsCommunicator clients)
    {
        _clients = clients;
    }
    
    public ClientIdentifier Create(Socket socket)
    {
        return new ClientIdentifier(socket, _clients.ClientIds);
    }
}