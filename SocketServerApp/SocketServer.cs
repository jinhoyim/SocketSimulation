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
        private readonly int _startMinConnections = 5;
        
        private readonly Lock _lock = new Lock();
        private readonly ConcurrentDictionary<string, SocketCommunicator> _clients = new();

        private int _increment;

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
                var connectionAcceptor = new ConnectionAcceptor(client, _clients.Keys);
                (var accepted, clientId) = await connectionAcceptor.AcceptAsync(cancellationToken);
                if (accepted)
                {
                    var communicator = new SocketCommunicator(client);
                    _clients[clientId] = communicator;
                    if (CanInitAndFirstSend())
                    {
                        var initialData = CreateInitialDataRecord();
                        await FirstSendAsync(initialData, cancellationToken);
                    }

                    await communicator.ReceiveAsync(cancellationToken);
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

        private DataRecord CreateInitialDataRecord()
        {
            var initValue = 1;
            var lockTime = LockTime.From(DateTime.Now.AddSeconds(2));
            string id;
            
            lock (_lock)
            {
                _increment++;
                id = _increment.ToString();
            }
            return new DataRecord(id, lockTime, string.Empty, initValue);
        }
        
        private async Task FirstSendAsync(DataRecord record, CancellationToken cancellationToken)
        {
            var tasks = _clients.Values.Select(client =>
                    client.SendRecordAsync(record, cancellationToken));
            await Task.WhenAll(tasks);
        }

        private bool CanInitAndFirstSend()
        {
            lock (_lock)
            {
                return !_isStarted && _clients.Count >= _startMinConnections;
            } 
        }
    }
}
