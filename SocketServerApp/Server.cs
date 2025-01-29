using System.Net;
using System.Net.Sockets;
using SocketServerApp.Communication;
using SocketServerApp.Processing;
using SocketServerApp.Store;

namespace SocketServerApp
{
    public class Server
    {
        private readonly LingerOption _lingerOption = new(true, 10);
        private readonly int _socketConnectionQueue;
        
        private readonly IPEndPoint _ipEndPoint;
        private readonly int _startConnectionCount;
        private readonly int _endCount;
        private readonly TimeSpan _initLockTime;
        private readonly TimeSpan _serverTerminatedDelay;
        private readonly CancellationTokenSource _cts;
        private readonly int _processorCount;

        private Server(
            IPEndPoint ipEndPoint,
            int startConnectionCount,
            int endCount,
            TimeSpan initLockTime,
            int socketConnectionQueue,
            TimeSpan serverTerminatedDelay,
            int processorCount,
            CancellationTokenSource cts)
        {
            _ipEndPoint = ipEndPoint;
            _startConnectionCount = startConnectionCount;
            _endCount = endCount;
            _initLockTime = initLockTime;
            _socketConnectionQueue = socketConnectionQueue;
            _serverTerminatedDelay = serverTerminatedDelay;
            _processorCount = processorCount;
            _cts = cts;
        }

        public static Server Create(ServerConfig config, CancellationTokenSource cts)
        {
            return new Server(
                config.IpEndPoint,
                config.StartConnectionCount,
                config.EndCount,
                config.InitLockTime,
                config.SocketConnectionQueue,
                config.ServerTerminatedDelay,
                config.ProcessorCount,
                cts);
        }

        internal async Task StartAsync(CancellationToken cancellationToken)
        {
            using var connectionListener = ServerListener.Create(_ipEndPoint, _lingerOption, _socketConnectionQueue);
            var clients = new AllCilentsCommunicator();
            var serverTerminator = new ServerTerminator(clients, _serverTerminatedDelay, _cts);
            var dataStore = new DataStore(_endCount);
            var startStateStore = new StartStateStore(clients, _startConnectionCount);

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var client = await connectionListener.AcceptAsync(cancellationToken);
                    _ = Task.Run(async () =>
                    {
                        await ClientHandleAsync(
                            client,
                            serverTerminator,
                            dataStore,
                            clients,
                            startStateStore,
                            cancellationToken);
                    }, cancellationToken);
                }
                catch (InvalidOperationException invalidOperationException)
                {
                    Console.WriteLine($"Socket cannot to accept. {invalidOperationException}");
                }
            }
            Console.WriteLine("Listener socket closed.");
        }

        private async Task ClientHandleAsync(
            Socket clientSocket,
            ServerTerminator serverTerminator,
            DataStore dataStore,
            AllCilentsCommunicator clients,
            StartStateStore startStateStore,
            CancellationToken cancellationToken)
        {
            var clientId = string.Empty;
            try
            {
                var identifier = new ClientIdentifier(clientSocket, clients.ClientIds);
                await identifier.IdentifyClientAsync(cancellationToken);
                clientId = identifier.ClientId;

                if (!identifier.IsVerified || string.IsNullOrEmpty(clientId))
                {
                    return;
                }

                var communicator = new ClientCommunicator(clientId, clientSocket);
                clients.Add(clientId, communicator);
                
                var worker = new ServerWorker(
                    dataStore,
                    communicator,
                    clients,
                    serverTerminator,
                    startStateStore,
                    _initLockTime
                );
                await worker.RunAsync(_processorCount, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                #if DEBUG
                Console.WriteLine("Client Socket Operation cancelled.");
                #endif
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
                Console.WriteLine($"ClientId: {clientId} is closed.");
                if (clientSocket.Connected)
                {
                    clientSocket.Shutdown(SocketShutdown.Both);
                }
                clientSocket.Dispose();

                if (!string.IsNullOrEmpty(clientId))
                {
                    clients.TryRemove(clientId);
                }

                if (clients.IsEmpty)
                {
                    await _cts.CancelAsync();
                }
            }
        }
    }
}
