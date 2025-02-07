using System.Net.Sockets;
using Microsoft.Extensions.Options;
using SocketServerApp.Communication;
using SocketServerApp.Factories;

namespace SocketServerApp;

public class ClientHandler
{
    private readonly AllCilentsCommunicator _clients;
    private readonly int _processorCount;
    private readonly ClientIdentifierFactory _clientIdentifierFactory;
    private readonly ClientCommunicatorFactory _clientCommunicatorFactory;
    private readonly ServerWorkerFactory _serverWorkerFactory;
    private readonly CancellationTokenSource _cts;

    public ClientHandler(
        AllCilentsCommunicator clients,
        IOptions<ServerConfig> config,
        ClientIdentifierFactory clientIdentifierFactory,
        ClientCommunicatorFactory clientCommunicatorFactory,
        ServerWorkerFactory serverWorkerFactory,
        CancellationTokenSource cancellationTokenSource)
    {
        _clients = clients;
        _processorCount = config.Value.ProcessorCount;
        _clientIdentifierFactory = clientIdentifierFactory;
        _clientCommunicatorFactory = clientCommunicatorFactory;
        _serverWorkerFactory = serverWorkerFactory;
        _cts = cancellationTokenSource;
    }
    
    public async Task HandleAsync(Socket clientSocket, CancellationToken cancellationToken)
    {
        var clientId = string.Empty;
        try
        {
            var identifier = _clientIdentifierFactory.Create(clientSocket);
            await identifier.IdentifyClientAsync(cancellationToken);
            clientId = identifier.ClientId;
            if (!identifier.IsVerified || string.IsNullOrEmpty(clientId))
            {
                return;
            }

            var communicator = _clientCommunicatorFactory.Create(clientId, clientSocket);
            _clients.Add(clientId, communicator);
            
            var worker = _serverWorkerFactory.Create(communicator);
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
                _clients.TryRemove(clientId);
            }

            if (_clients.IsEmpty)
            {
                await _cts.CancelAsync();
            }
        }
    }
}