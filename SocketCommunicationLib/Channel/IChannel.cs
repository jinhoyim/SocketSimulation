namespace SocketCommunicationLib.Channel;

public interface IChannel<T>
{
    ValueTask WriteAsync(T item, CancellationToken cancellationToken = default);
    IAsyncEnumerable<T> ReadAllAsync(CancellationToken cancellationToken = default);
    void Complete(Exception? error = null);
    Task ReaderCompletion();
}