using System.Net.Sockets;
using Microsoft.Extensions.Options;
using SocketClientApp.Communication;

namespace SocketClientApp;

public class ConnectorFactory
{
    private readonly string _clientId;
    
    public ConnectorFactory(IOptions<ClientConfig> config)
    {
        _clientId = config.Value.ClientId;
    }
    
    public Connector Create(Socket socket)
    {
        return new Connector(socket, _clientId);
    }
}