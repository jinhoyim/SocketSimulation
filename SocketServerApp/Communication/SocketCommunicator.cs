using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using SocketCommunicationLib.Contract;

namespace SocketServerApp.Communication;

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

    public async Task SendStringAsync(string message, CancellationToken cancellationToken)
    {
        var messageBytes = Encoding.UTF8.GetBytes(message);
        await _socket.SendAsync(messageBytes, SocketFlags.None, cancellationToken);
    }
}