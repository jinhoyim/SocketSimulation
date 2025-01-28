using System.Net;
using System.Net.Sockets;
using System.Text;
using SocketClientApp.Communication;
using SocketClientApp.Output;
using SocketClientApp.Processing;
using SocketClientApp.Store;
using SocketCommunicationLib;
using SocketCommunicationLib.Contract;

namespace SocketClientApp;

public class Client
{
    private const int LingerTimeSeconds = 10;
    private readonly string _clientId;
    private readonly IPEndPoint _ipEndPoint;
    private readonly CancellationTokenSource _cts;

    private Client(string clientId, IPAddress ipAddress, int port, CancellationTokenSource cts)
    {
        _clientId = clientId;
        _ipEndPoint = new IPEndPoint(ipAddress, port);
        _cts = cts;
    }

    internal static Client Create(string clientId, IPAddress ipAddress, int port, CancellationTokenSource cts)
    {
        return new Client(clientId, ipAddress, port, cts);
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

            var store = new DataStore();
            var logger = new OutputWriter();
            
            if (connected)
            {
                var communicator = new SocketCommunicator(server);
                var jobChannel = new ClientJobChannel<string>();
                var processor = new ClientJobProcessor(
                    jobChannel,
                    communicator,
                    new MessageConverter(),
                    new QueryResultHandler(communicator, store, logger),
                    new LockTimeHandler(communicator),
                    _cts);
                
                var messageListener = new SocketListener(
                    server,
                    new SocketMessageStringExtractor(
                        ProtocolConstants.Eom,
                        Encoding.UTF8),
                    jobChannel
                );

                try
                {
                    var processTask = processor.ProcessAsync(cancellationToken);
                    var listenTask = messageListener.ListenAsync(cancellationToken);
                    await Task.WhenAll(processTask, listenTask);
                }
                catch (OperationCanceledException)
                {
                    Console.WriteLine("Operation cancelled.");
                }
                Console.WriteLine("End handle.");
            }
            else
            {
                Console.WriteLine($"Connection failed and application stop. {errorMessage}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"System Error : {ex}");
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