using System.Net.Sockets;
using SocketCommunicationLib.Channel;
using SocketCommunicationLib.Contract;

namespace SocketCommunicationLib;

public class SocketListener
{
    private readonly Socket _socket;
    private readonly SocketMessageStringExtractor _socketMessageStringExtractor;

    public SocketListener(
        Socket socket,
        SocketMessageStringExtractor socketMessageStringExtractor)
    {
        _socket = socket;
        _socketMessageStringExtractor = socketMessageStringExtractor;
    }

    public async Task ListenAsync(IChannel<Message> outputChannel, CancellationToken cancellationToken)
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
                await outputChannel.WriteAsync(message, cancellationToken);
            }
        }
    }
}