using System.Net;
using System.Net.Sockets;
using SocketClientApp.Communication;

namespace SocketClientApp;

public class Client
{
    private const int LingerTimeSeconds = 10;
    private readonly string _clientId;
    private readonly IPEndPoint _ipEndPoint;
    private readonly int _maxMilliseconds;
    private readonly CancellationTokenSource _cts;
    private readonly int _processorCount;

    private Client(string clientId, IPEndPoint ipEndPoint, int maxMilliseconds, int processorCount, CancellationTokenSource cts)
    {
        _clientId = clientId;
        _ipEndPoint = ipEndPoint;
        _maxMilliseconds = maxMilliseconds;
        _processorCount = processorCount;
        _cts = cts;
    }
    
    public static Client Create(ClientConfig config, CancellationTokenSource cts)
    {
        return new Client(
            config.ClientId,
            config.ServerIpEndPoint,
            config.MaxMilliseconds,
            config.ProcessorCount,
            cts);
    }

    internal async Task StartAsync()
    {
        var cancellationToken = _cts.Token;
        
        using var server = new Socket(
            _ipEndPoint.AddressFamily,
            SocketType.Stream,
            ProtocolType.Tcp);
        server.LingerState = new LingerOption(true, LingerTimeSeconds);

        try
        {
            await server.ConnectAsync(_ipEndPoint, cancellationToken);

            var connector = new Connector(server, _clientId);
            (bool connected, string errorMessage) = await connector.ConnectAsync(cancellationToken);

            if (!connected)
            {
                Console.WriteLine($"Connection failed and application stop. {errorMessage}");
            }

            var worker = new ClientWorker(server, _maxMilliseconds, _cts);
            await worker.RunAsycn(_processorCount, cancellationToken);
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