using System.Net.Sockets;
using Microsoft.Extensions.Options;
using SocketServerApp.Communication;
using SocketServerApp.Processing;
using SocketServerApp.Store;

namespace SocketServerApp
{
    public class Server : ISocketServer
    {
        private readonly AllCilentsCommunicator _clients;
        private readonly IDataStore _dataStore;
        private readonly StartStateStore _startStateStore;
        private readonly ServerListener _connectionListener;
        private readonly ServerTerminator _serverTerminator;
        private readonly ServerConfig _config;
        private readonly CancellationTokenSource _cts;

        public Server(
            AllCilentsCommunicator clients,
            IDataStore dataStore,
            StartStateStore startStateStore,
            ServerListener connectionListener,
            ServerTerminator serverTerminator,
            IOptions<ServerConfig> config,
            CancellationTokenSource cts)
        {
            _clients = clients;
            _dataStore = dataStore;
            _startStateStore = startStateStore;
            _connectionListener = connectionListener;
            _serverTerminator = serverTerminator;
            _config = config.Value;
            _cts = cts;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var client = await _connectionListener.AcceptAsync(cancellationToken);
                    _ = Task.Run(async () =>
                    {
                        await ClientHandleAsync(
                            client,
                            _serverTerminator,
                            _dataStore,
                            _clients,
                            _startStateStore,
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
            IDataStore dataStore,
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
                    _config.InitLockTime
                );
                await worker.RunAsync(_config.ProcessorCount, cancellationToken);
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
