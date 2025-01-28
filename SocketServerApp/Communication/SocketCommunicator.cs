using System.Net.Sockets;
using System.Text;
using SocketCommunicationLib;
using SocketCommunicationLib.Contract;

namespace SocketServerApp.Communication;

public class SocketCommunicator
{
    private readonly Socket _socket;

    public SocketCommunicator(Socket socket)
    {
        _socket = socket;
    }

    public async Task SendEmptyDataAsync(string content, CancellationToken cancellationToken)
    {
        await SendStringAsync(content, ProtocolConstants.ErrorEmptyData, cancellationToken);
    }

    public async Task SendDataLockedAsync(string content, CancellationToken cancellationToken)
    {
        await SendStringAsync(content, ProtocolConstants.LockTime, cancellationToken);
    }
    
    public async Task SendBadRequestAsync(string requestPrefix, CancellationToken cancellationToken)
    {
        string message = $"Request: {requestPrefix}";
        await SendStringAsync(message, ProtocolConstants.ErrorBadRequest, cancellationToken);
    }

    public async Task SendLockTimeAsync<T>(T data, CancellationToken cancellationToken)
    {
        await SendAsync(data, ProtocolConstants.LockTime, cancellationToken);
    }

    public async Task SendQueryResultAsync<T>(T data, CancellationToken cancellationToken)
    {
        await SendAsync(data, ProtocolConstants.DataRecordWithNext, cancellationToken);
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