using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Security;
using System.Text;

namespace SocketServerApp
{
    public class SocketServer
    {
        private readonly LingerOption _lingerOption = new(true, 10);
        private readonly int _socketConnectionQueue = 1000;
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

        internal async Task StartAsync(CancellationToken cancellationToken)
        {
            using var connectionListener = ConnectionListener.Create(_ipEndPoint, _lingerOption, _socketConnectionQueue);
            
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var client = await connectionListener.AcceptAsync(cancellationToken);
                    _ = Task.Run(async () =>
                    {
                        await ClientHandleAsync(client, cancellationToken);
                        Console.WriteLine("Client Socket closed.");
                    }, cancellationToken);
                }
                catch (InvalidOperationException invalidOperationException)
                {
                    Console.WriteLine($"Socket cannot to accept. {invalidOperationException}");
                }
            }
            Console.WriteLine("Listener socket closed.");
        }

        private async Task ClientHandleAsync(Socket client, CancellationToken cancellationToken)
        {
            string clientId = string.Empty;
            try
            {
                var identifier = new ClientIdentifier(client, _clients.Keys);
                await identifier.IdentifyClientAsync(cancellationToken);
                clientId = identifier.ClientId;
                
                if (!identifier.IsVerified || string.IsNullOrEmpty(clientId))
                {
                    return;
                }

                var communicator = new SocketCommunicator(client);
                _clients[clientId] = communicator;
                
                if (CanInitAndFirstSend())
                {
                    var initialData = CreateInitialDataRecord();
                    await FirstSendAsync(initialData, cancellationToken);
                }

                await communicator.ReceiveAsync(cancellationToken);
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

                if (!string.IsNullOrEmpty(clientId))
                {
                    _clients.TryRemove(clientId, out _);
                }

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
