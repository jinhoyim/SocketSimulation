namespace SocketServerApp;

public interface ISocketServer
{
    Task StartAsync(CancellationToken cancellationToken);
}