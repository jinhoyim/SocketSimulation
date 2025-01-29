using System.Net.Sockets;
using System.Text;
using SocketCommunicationLib.Contract;

namespace SocketCommunicationLib;

public class SocketCommunicator
{
    private readonly Socket _socket;
    
    public Socket Socket => _socket;

    protected SocketCommunicator(Socket socket)
    {
        _socket = socket;
    }
    
    public async Task SendAsync<T>(T data, string prefix, CancellationToken cancellationToken)
    {
        var json = JsonUtils.Serialize(data);
        string message = $"{prefix}{json}{ProtocolConstants.Eom}";
        var messageBytes = Encoding.UTF8.GetBytes(message);
        await _socket.SendAsync(messageBytes, SocketFlags.None, cancellationToken);
    }
    
    public async Task SendStringAsync(string content, string prefix, CancellationToken cancellationToken)
    {
        var message = $"{prefix}{content}{ProtocolConstants.Eom}";
        var messageBytes = Encoding.UTF8.GetBytes(message);
        await _socket.SendAsync(messageBytes, SocketFlags.None, cancellationToken);
    }
}