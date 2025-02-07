using System.Net.Sockets;
using Microsoft.Extensions.Options;
using SocketClientApp.Channel;
using SocketCommunicationLib.Contract;

namespace SocketClientApp;

public class ClientWorker
{
    private readonly ClientCommunicatorFactory _clientCommunicatorFactory;
    private readonly SocketListenerFactory _socketListenerFactory;
    private readonly ClientJobProcessorFactory _clientJobProcessorFactory;
    private readonly int _processorCount;

    public ClientWorker(
        ClientCommunicatorFactory clientCommunicatorFactory,
        SocketListenerFactory socketListenerFactory,
        ClientJobProcessorFactory clientJobProcessorFactory,
        IOptions<ClientConfig> config)
    {
        _clientCommunicatorFactory = clientCommunicatorFactory;
        _socketListenerFactory = socketListenerFactory;
        _clientJobProcessorFactory = clientJobProcessorFactory;
        _processorCount = config.Value.ProcessorCount;
    }

    public async Task RunAsync(Socket socket, CancellationToken cancellationToken)
    {
        var communicator = _clientCommunicatorFactory.Create(socket);
        var messageListener = _socketListenerFactory.Create(communicator);
        
        var jobChannel = new ClientJobChannel<Message>();

        var tasks = Enumerable.Range(0, _processorCount)
            .Select(t => _clientJobProcessorFactory.Create(communicator)
                .ProcessAsync(jobChannel, cancellationToken))
            .ToList();
        tasks.Add(messageListener.ListenAsync(jobChannel, cancellationToken));
        await Task.WhenAll(tasks);
    }
}