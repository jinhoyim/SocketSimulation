using System.Net;
using System.Net.Sockets;
using System.Text;
using SocketCommunicationLib;

namespace SocketClientApp;

public class SocketClient
{
    private const int LingerTimeSeconds = 10;
    private readonly string _clientId;
    private readonly IPEndPoint _ipEndPoint;
    private readonly CancellationTokenSource _cts;

    private SocketClient(string clientId, IPAddress ipAddress, int port, CancellationTokenSource cts)
    {
        _clientId = clientId;
        _ipEndPoint = new IPEndPoint(ipAddress, port);
        _cts = cts;
    }

    internal static SocketClient Create(string clientId, IPAddress ipAddress, int port, CancellationTokenSource cts)
    {
        return new SocketClient(clientId, ipAddress, port, cts);
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

            if (connected)
            {
                var communicator = new SocketCommunicator(server);
                
                var jobChannel = new ClientJobChannel<string>();
                
                var processor = new ClientJobProcessor(jobChannel, communicator, _cts);
                
                var messageListener = new MessageListener(
                    server,
                    new MessageStringExtractor(
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