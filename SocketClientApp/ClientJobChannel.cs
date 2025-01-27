using System.Threading.Channels;
using SocketCommunicationLib;

namespace SocketClientApp;

public class ClientJobChannel<T> : IChannel<T>
{
    private readonly Channel<T> _channel;

    public ClientJobChannel()
    {
        _channel = Channel.CreateUnbounded<T>();
    }
    
    public ValueTask WriteAsync(T item, CancellationToken cancellationToken = default)
    {
        return _channel.Writer.WriteAsync(item, cancellationToken);
    }

    public IAsyncEnumerable<T> ReadAllAsync(CancellationToken cancellationToken = default)
    {
        return _channel.Reader.ReadAllAsync(cancellationToken);
    }

    public void Complete(Exception? error = null)
    {
        _channel.Writer.Complete(error);
    }
    
    public Task ReaderCompletion()
    {
        return _channel.Reader.Completion;
    }
}