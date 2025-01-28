using SocketCommunicationLib.Channel;
using SocketCommunicationLib.Contract;
using SocketServerApp.Store;
using static SocketCommunicationLib.Contract.DataProtocolConstants;

namespace SocketServerApp.Processing;

public class ServerJobProcessor
{
    private readonly IChannel<Message> _channel;
    private readonly DataStore _dataStore;
    private readonly MessageConverter _messageConverter;
    private readonly string _clientId;
    private readonly QueryDataHandler _queryHandler;
    private readonly ServerTerminator _serverTerminator;

    public ServerJobProcessor(
        IChannel<Message> channel,
        string clientId,
        DataStore dataStore,
        QueryDataHandler queryHandler,
        MessageConverter messageConverter,
        ServerTerminator serverTerminator)
    {
        _channel = channel;
        _clientId = clientId;
        _dataStore = dataStore;
        _queryHandler = queryHandler;
        _messageConverter = messageConverter;
        _serverTerminator = serverTerminator;
    }

    public async Task ProcessAsync(CancellationToken cancellationToken)
    {
        await foreach (Message message in _channel.ReadAllAsync(cancellationToken))
        {
            if (message == Message.Empty) continue;

            switch (message.Type)
            {
                case QueryData when message.Content is string recordId:
                    await _queryHandler.HandleAsync(recordId, cancellationToken); 
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