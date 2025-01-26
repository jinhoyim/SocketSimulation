using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using SocketCommunicationLib;

namespace SocketServerApp;

public class SocketCommunicator
{
    private readonly Socket _socket;

    public SocketCommunicator(Socket socket)
    {
        _socket = socket;
    }

    public async Task SendRecordAsync(DataRecord record, CancellationToken cancellationToken)
    {
        var json = JsonSerializer.Serialize(record);
        string message = $"{ProtocolConstants.LockTime}{json}{ProtocolConstants.Eom}";
        var messageBytes = Encoding.UTF8.GetBytes(message);
        await _socket.SendAsync(messageBytes, SocketFlags.None, cancellationToken);
    }

    public async Task<string> ReceiveAsync(CancellationToken cancellationToken)
    {
        var buffer = new byte[1024];
        var receivedDataLength = await _socket.ReceiveAsync(buffer, SocketFlags.None, cancellationToken);
        var receivedData = Encoding.UTF8.GetString(buffer, 0, receivedDataLength);
        
        if (receivedData.EndsWith(ProtocolConstants.Eom))
        {
            var action = "<|QUERY|>";
            var eomPosition = receivedData.IndexOf(ProtocolConstants.Eom, 0, StringComparison.Ordinal);
            return receivedData[action.Length..eomPosition];
        }

        return string.Empty;
    }
}