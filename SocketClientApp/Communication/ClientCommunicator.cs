using System.Net.Sockets;
using SocketCommunicationLib;
using SocketCommunicationLib.Contract;
using SocketCommunicationLib.Model;

namespace SocketClientApp.Communication;

public class ClientCommunicator(Socket socket) : SocketCommunicator(socket)
{
    public async Task SendQueryAsync(string id, CancellationToken cancellationToken)
    {
        await SendStringAsync(id, DataProtocolConstants.QueryData, cancellationToken);
    }

    public async Task SendNextDataAsync(NextDataValue data, CancellationToken cancellationToken)
    {
        await SendAsync(data, DataProtocolConstants.NextData, cancellationToken);
    }
}