using Microsoft.Extensions.DependencyInjection;

namespace SocketServerApp
{
    public class Server : ISocketServer
    {
        private readonly ServerListener _connectionListener;   
        private readonly IServiceProvider _serviceProvider;

        public Server(
            ServerListener connectionListener,
            IServiceProvider serviceProvider)
        {
            _connectionListener = connectionListener;
            _serviceProvider = serviceProvider;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var client = await _connectionListener.AcceptAsync(cancellationToken);
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            using var scope = _serviceProvider.CreateScope();
                            var clientHandler = scope.ServiceProvider.GetRequiredService<ClientHandler>();
                            await clientHandler.HandleAsync(client, cancellationToken);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Client handler scope DI failed: {ex.Message}");
                        }
                    }, cancellationToken);
                }
                catch (InvalidOperationException invalidOperationException)
                {
                    Console.WriteLine($"Socket cannot to accept. {invalidOperationException}");
                }
            }
            Console.WriteLine("Listener socket closed.");
        }
    }
}