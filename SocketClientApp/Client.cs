using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace SocketClientApp;

public class Client : ISocketClient
{
    private readonly IPEndPoint _ipEndPoint;
    private readonly LingerOption _lingerOption;
    private readonly ConnectorFactory _connectorFactory;
    private readonly CancellationTokenSource _cts;
    private readonly IServiceProvider _serviceProvider;

    public Client(
        IOptions<ClientConfig> config,
        ConnectorFactory connectorFactory,
        IServiceProvider serviceProvider,
        CancellationTokenSource cancellationTokenSource
        )
    {
        _ipEndPoint = config.Value.ServerIpEndPoint;
        _lingerOption = config.Value.LingerOption;
        _connectorFactory = connectorFactory;
        _serviceProvider = serviceProvider;
        _cts = cancellationTokenSource;
    }

    public async Task StartAsync()
    {
        var cancellationToken = _cts.Token;
        
        using var server = new Socket(
            _ipEndPoint.AddressFamily,
            SocketType.Stream,
            ProtocolType.Tcp);
        server.LingerState = _lingerOption;

        try
        {
            await server.ConnectAsync(_ipEndPoint, cancellationToken);

            var connector = _connectorFactory.Create(server);
            (bool connected, string errorMessage) = await connector.ConnectAsync(cancellationToken);

            if (!connected)
            {
                Console.WriteLine($"Connection failed and application stop. {errorMessage}");
                return;
            }

            using var scope = _serviceProvider.CreateScope();
            var worker = scope.ServiceProvider.GetRequiredService<ClientWorker>();
            await worker.RunAsync(server, cancellationToken);
        }
        finally
        {
            if (server.Connected)
            {
                server.Shutdown(SocketShutdown.Both);
            }
        }
    }
}