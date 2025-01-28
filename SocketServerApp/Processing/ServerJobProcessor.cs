using SocketCommunicationLib.Channel;
using SocketCommunicationLib.Contract;
using SocketServerApp.Communication;
using SocketServerApp.Store;
using static SocketCommunicationLib.Contract.ProtocolConstants;

namespace SocketServerApp.Processing;

public class ServerJobProcessor
{
    private readonly IChannel<string> _channel;
    private readonly CancellationTokenSource _cts;
    private readonly DataStore _dataStore;
    private readonly SocketsCommunicator _socketsCommunicator;
    private readonly MessageConverter _messageConverter;
    private readonly SocketCommunicator _communicator;
    private readonly string _clientId;
    private readonly QueryDataHandler _queryHandler;
    private readonly ServerTerminator _serverTerminator;

    public ServerJobProcessor(
        IChannel<string> channel,
        string clientId,
        SocketCommunicator communicator,
        DataStore dataStore,
        SocketsCommunicator socketsCommunicator,
        QueryDataHandler queryHandler,
        MessageConverter messageConverter,
        ServerTerminator serverTerminator,
        CancellationTokenSource cts)
    {
        _channel = channel;
        _clientId = clientId;
        _communicator = communicator;
        _dataStore = dataStore;
        _socketsCommunicator = socketsCommunicator;
        _queryHandler = queryHandler;
        _messageConverter = messageConverter;
        _serverTerminator = serverTerminator;
        _cts = cts;
    }

    public async Task ProcessAsync(CancellationToken cancellationToken)
    {
        await foreach (var item in _channel.ReadAllAsync(cancellationToken))
        {
            var message = _messageConverter.Convert(item);
            if (message == Message.Empty) continue;

            switch (message.Type)
            {
                case QueryData:
                    await _queryHandler.HandleAsync(message.Content, cancellationToken); 
                    break;
                case NextData:
                    Console.WriteLine($"Next data received: {message}");
                    break;
            }

            // var savedCount = _dataStore.Save();
            // savedCount++;
            // if (savedCount == 4)
            // {
            // await _serverTerminator.ServerTerminate(cancellationToken);
            // }
        }
    }
}