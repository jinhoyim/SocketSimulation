using System.Net.Sockets;
using SocketCommunicationLib.Channel;

namespace SocketCommunicationLib;

public class SocketListener
{
    private readonly Socket _socket;
    private readonly SocketMessageStringExtractor _socketMessageStringExtractor;
    private readonly IChannel<string> _channel;

    public SocketListener(
        Socket socket,
        SocketMessageStringExtractor socketMessageStringExtractor,
        IChannel<string> channel)
    {
        _socket = socket;
        _socketMessageStringExtractor = socketMessageStringExtractor;
        _channel = channel;
    }

    public async Task ListenAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            var buffer = new byte[1024];
            var receivedDataLength = await _socket.ReceiveAsync(buffer, SocketFlags.None, cancellationToken);
            var messages = _socketMessageStringExtractor.AppendAndExtract(buffer, 0, receivedDataLength);
            foreach (var item in messages)
            {
                await _channel.WriteAsync(item, cancellationToken);
            }
        }
    }
}