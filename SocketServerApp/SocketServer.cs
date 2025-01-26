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
            
            using var listener = new Socket(
                        _ipEndPoint.AddressFamily,
                        SocketType.Stream,
                        ProtocolType.Tcp);
            listener.LingerState = new LingerOption(true, LingerTimeSeconds);
            listener.Bind(_ipEndPoint);
            listener.Listen(1000);

            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    var client = await listener.AcceptAsync(cancellationToken);
                    client.LingerState = new LingerOption(true, LingerTimeSeconds);

                    _ = Task.Run(async () =>
                    {
                        string clientId = string.Empty;
                        var connectionAcceptor = new ConnectionAcceptor(client, _clients);
                        bool accepted = false;
                        
                        try
                        {
                            (accepted, clientId) = await connectionAcceptor.AcceptAsync(cancellationToken);    
                            if (!accepted)
                            {
                                return;
                            }
                            _clients[clientId] = client;
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
            }
            Console.WriteLine("Listener socket closed.");
        }
    }
}
