using System.Net.Sockets;
using System.Text;
using SocketCommunicationLib;

namespace SocketClientApp;

public class SocketCommunicator
{
    private readonly Socket _socket;

    public SocketCommunicator(Socket socket)
    {
        _socket = socket;
    }
    
    public async Task SendRecordAsync(string content, CancellationToken cancellationToken)
    {
        string message = $"<|TEST|>{content}{ProtocolConstants.Eom}";
        var messageBytes = Encoding.UTF8.GetBytes(message);
        await _socket.SendAsync(messageBytes, SocketFlags.None, cancellationToken);
    }
}