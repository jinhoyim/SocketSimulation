﻿using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;
using SocketCommunicationLib;
using SocketCommunicationLib.Contract;
using SocketServerApp.Communication;
using SocketServerApp.Processing;
using SocketServerApp.Store;

namespace SocketServerApp
{
    public class Server
    {
        private readonly LingerOption _lingerOption = new(true, 10);
        private readonly int _socketConnectionQueue = 1000;
        private readonly IPEndPoint _ipEndPoint;
        private readonly CancellationTokenSource _cts;

        private bool _isStarted = false;
        private readonly int _startMinConnections = 5;
        
        private readonly Lock _lock = new Lock();
        private readonly ConcurrentDictionary<string, SocketCommunicator> _clients = new();
        private readonly DataStore _dataStore = new(10);
        private readonly SocketsCommunicator _socketsCommunicator;

        private Server(IPAddress ipAddress, int port, CancellationTokenSource cts)
        {
            _ipEndPoint = new IPEndPoint(ipAddress, port);
            _cts = cts;
            _socketsCommunicator = new SocketsCommunicator(_clients);
        }

        internal static Server Create(IPAddress ipAddress, int port, CancellationTokenSource cts)
        {
            return new Server(ipAddress, port, cts);
        }

        internal async Task StartAsync(CancellationToken cancellationToken)
        {
            using var connectionListener = ServerListener.Create(_ipEndPoint, _lingerOption, _socketConnectionQueue);
            
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
            var clientId = string.Empty;
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
                var jobChannel = new ServerJobChannel<string>();
                var processor = new ServerJobProcessor(
                    jobChannel,
                    clientId,
                    communicator,
                    _dataStore,
                    _socketsCommunicator,
                    new QueryDataHandler(clientId, communicator, _dataStore),
                    new MessageConverter(),
                    _cts);

                var messageListener = new SocketListener(
                    client,
                    new SocketMessageStringExtractor(
                        ProtocolConstants.Eom,
                        Encoding.UTF8),
                    jobChannel);

                var processTask = processor.ProcessAsync(cancellationToken);
                var listenTask = messageListener.ListenAsync(cancellationToken);
                
                if (CanInitAndFirstSend())
                {
                    var initialData = _dataStore.InitialDataRecord();
                    _dataStore.Save(initialData);
                    await FirstSendAsync(initialData, cancellationToken);
                    Console.WriteLine("Initial data recorded.");
                    lock (_lock)
                    {
                        _isStarted = true;
                    }
                }
                await Task.WhenAll(listenTask, processTask);
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine($"Operation cancelled. ClientId: {clientId} socket closed.");
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
        
        private async Task FirstSendAsync(DataRecord record, CancellationToken cancellationToken)
        {
            var tasks = _clients.Values.Select(client =>
                    client.SendLockTimeAsync(record, cancellationToken));
            await Task.WhenAll(tasks);
        }

        private bool CanInitAndFirstSend()
        {
            if (_isStarted) return false;
            lock (_lock)
            {
                return !_isStarted && _clients.Count >= _startMinConnections;
            } 
        }
    }
}
