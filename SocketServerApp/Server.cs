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
                    var clientSession = await _connectionListener.AcceptAsync(cancellationToken);
                    if (clientSession == null)
                    {
                        continue;
                    }
                    
                    _ = Task.Run(async () => await HandleAsync(clientSession, cancellationToken), cancellationToken);
                }
                catch (InvalidOperationException invalidOperationException)
                {
                    Console.WriteLine($"Socket cannot to accept. {invalidOperationException}");
                }
            }
            Console.WriteLine("Listener socket closed.");
        }

        private async Task HandleAsync(ClientSession clientSession, CancellationToken cancellationToken)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var clientHandler = scope.ServiceProvider.GetRequiredService<ClientHandler>();
                await clientHandler.HandleAsync(clientSession, cancellationToken);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Client handler scope DI failed: {ex.Message}");
            }
        }
    }
}