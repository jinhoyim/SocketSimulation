using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace SocketServerApp
{
    public class SocketServer
    {
        private const int LingerTimeSeconds = 10;
        private readonly IPEndPoint _ipEndPoint;
        private readonly CancellationTokenSource _cts;

        private bool _isStarted = false;
        private readonly int startMinConnections = 5;
        private readonly Lock _lock = new Lock();
        private readonly ConcurrentDictionary<string, Socket> _clients = new();

        public SocketServer(IPAddress ipAddress, int port, CancellationTokenSource cts)
        {
            _ipEndPoint = new IPEndPoint(ipAddress, port);
            _cts = cts;
        }

        internal static SocketServer Create(IPAddress ipAddress, int port, CancellationTokenSource cts)
        {
            return new SocketServer(ipAddress, port, cts);
        }

        internal async Task StartAsync()
        {
            var cancellationToken = _cts.Token;
            
            var listener = new Socket(
                _ipEndPoint.AddressFamily,
                SocketType.Stream,
                ProtocolType.Tcp);
            listener.LingerState = new LingerOption(true, LingerTimeSeconds);
            await ListenAsync(listener, cancellationToken);
        }

        private async Task ListenAsync(Socket listener, CancellationToken cancellationToken)
        {
            try
            {
                listener.Bind(_ipEndPoint);
                listener.Listen(1000);
                
                while (!cancellationToken.IsCancellationRequested)
                {
                    var client = await listener.AcceptAsync(cancellationToken);
                    client.LingerState = new LingerOption(true, LingerTimeSeconds);

                    _ = Task.Run(async () =>
                    {
                        await ClientHandleAsync(client, cancellationToken);
                        Console.WriteLine("Client Socket closed.");
                    }, cancellationToken);
                }
            }
            catch (OperationCanceledException oce)
            {
#if DEBUG
                Console.WriteLine($"Operation Canceled : {oce.Message}");
#endif
            }
            catch (Exception ex)
            {
                Console.WriteLine($"System Error : {ex}");
            }
            finally
            {
                if (listener.Connected)
                {
                    listener.Shutdown(SocketShutdown.Both);
                }
                listener.Dispose();
            }
            Console.WriteLine("Listener socket closed.");
        }

        private async Task ClientHandleAsync(Socket client, CancellationToken cancellationToken)
        {
            string clientId = string.Empty;
            try
            {
                var connectionAcceptor = new ConnectionAcceptor(client, _clients);
                (var accepted, clientId) = await connectionAcceptor.AcceptAsync(cancellationToken);
                if (accepted)
                {
                    await ReceiveAsync(client, clientId, cancellationToken);
                }
            }
            catch (SocketException se)
            {
                Console.WriteLine($"Socket Error : {se.SocketErrorCode}({se.ErrorCode}) {se.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"System Error : {ex}");
            }
            finally
            {
                if (client.Connected)
                {
                    client.Shutdown(SocketShutdown.Both);
                }
                client.Dispose();

                _clients.TryRemove(clientId, out _);
                if (_clients.IsEmpty)
                {
                    await _cts.CancelAsync();
                }
            }
        }

        private async Task ReceiveAsync(Socket client, string clientId, CancellationToken cancellationToken)
        {
            _clients[clientId] = client;
                        
            bool isStarted;
            lock (_lock)
            {
                isStarted = _isStarted;
            }

            if (!isStarted && _clients.Count >= startMinConnections)
            {
                
            }
        }
    }
}
