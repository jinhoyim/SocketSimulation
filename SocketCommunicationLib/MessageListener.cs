using System.Net.Sockets;

namespace SocketCommunicationLib;

public class MessageListener
{
    private readonly Socket _socket;
    private readonly MessageStringExtractor _messageStringExtractor;
    private readonly IChannel<string> _channel;

    public MessageListener(
        Socket socket,
        MessageStringExtractor messageStringExtractor,
        IChannel<string> channel)
    {
        _socket = socket;
        _messageStringExtractor = messageStringExtractor;
        _channel = channel;
    }

    public async Task ListenAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            var buffer = new byte[1024];
            var receivedDataLength = await _socket.ReceiveAsync(buffer, SocketFlags.None, cancellationToken);
            var messages = _messageStringExtractor.AppendAndExtract(buffer, 0, receivedDataLength);
            foreach (var item in messages)
            {
                await _channel.WriteAsync(item, cancellationToken);
            }
        }
    }
}