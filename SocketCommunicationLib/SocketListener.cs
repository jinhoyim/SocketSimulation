using System.Net.Sockets;
using SocketCommunicationLib.Channel;
using SocketCommunicationLib.Contract;

namespace SocketCommunicationLib;

public class SocketListener
{
    private readonly Socket _socket;
    private readonly SocketMessageStringExtractor _socketMessageStringExtractor;
    private readonly IChannel<Message> _channel;

    public SocketListener(
        Socket socket,
        SocketMessageStringExtractor socketMessageStringExtractor,
        IChannel<Message> channel)
    {
        _socket = socket;
        _socketMessageStringExtractor = socketMessageStringExtractor;
        _channel = channel;
    }

    public async Task ListenAsync(CancellationToken cancellationToken)
    {
        MessageConverter converter = new MessageConverter();
        while (!cancellationToken.IsCancellationRequested)
        {
            var buffer = new byte[1024];
            var receivedDataLength = await _socket.ReceiveAsync(buffer, SocketFlags.None, cancellationToken);
            var messages = _socketMessageStringExtractor.AppendAndExtract(buffer, 0, receivedDataLength);
            foreach (var item in messages)
            {
                Message message = converter.Convert(item);
                await _channel.WriteAsync(message, cancellationToken);
            }
        }
    }
}