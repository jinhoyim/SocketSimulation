using System.Net.Sockets;
using System.Text;
using SocketCommunicationLib;
using SocketCommunicationLib.Contract;

namespace SocketClientApp.Communication;

public class SocketCommunicator
{
    private readonly Socket _socket;

    public SocketCommunicator(Socket socket)
    {
        _socket = socket;
    }
    
    public async Task SendQueryAsync(DataRecord dataRecord, CancellationToken cancellationToken)
    {
        await SendAsync(dataRecord, ProtocolConstants.QueryData, cancellationToken);
    }

    public async Task SendNextDataAsync<T>(T data, CancellationToken cancellationToken)
    {
        await SendAsync(data, ProtocolConstants.NextData, cancellationToken);
    }
    
    private async Task SendAsync<T>(T data, string prefix, CancellationToken cancellationToken)
    {
        var json = JsonUtils.Serialize(data);
        string message = $"{prefix}{json}{ProtocolConstants.Eom}";
        var messageBytes = Encoding.UTF8.GetBytes(message);
        await _socket.SendAsync(messageBytes, SocketFlags.None, cancellationToken);
    }
}