using System.Net.Sockets;
using Microsoft.Extensions.Options;
using SocketServerApp.Communication;
using SocketServerApp.Factories;

namespace SocketServerApp;

public class ClientHandler
{
    private readonly AllCilentsCommunicator _clients;
    private readonly int _processorCount;
    private readonly ClientCommunicatorFactory _clientCommunicatorFactory;
    private readonly ServerWorkerFactory _serverWorkerFactory;

    public ClientHandler(
        AllCilentsCommunicator clients,
        IOptions<ServerConfig> config,
        ClientCommunicatorFactory clientCommunicatorFactory,
        ServerWorkerFactory serverWorkerFactory)
    {
        _clients = clients;
        _processorCount = config.Value.ProcessorCount;
        _clientCommunicatorFactory = clientCommunicatorFactory;
        _serverWorkerFactory = serverWorkerFactory;
    }
    
    public async Task HandleAsync(ClientSession session, CancellationToken cancellationToken)
    {
        try
        {
            var communicator = _clientCommunicatorFactory.Create(session);
            _clients.Add(session.ClientId, communicator);
            
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
            Console.WriteLine($"ClientId: {session.ClientId} is closed.");
            session.Dispose();

            if (!string.IsNullOrEmpty(session.ClientId))
            {
                _clients.TryRemove(session.ClientId);
            }

            if (_clients.IsEmpty)
            {
                Console.WriteLine("Clients are empty.");
            }
        }
    }
}