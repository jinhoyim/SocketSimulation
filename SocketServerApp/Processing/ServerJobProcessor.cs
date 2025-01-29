using SocketCommunicationLib.Channel;
using SocketCommunicationLib.Contract;
using SocketCommunicationLib.Model;
using SocketServerApp.Communication;
using SocketServerApp.Store;
using static SocketCommunicationLib.Contract.DataProtocolConstants;

namespace SocketServerApp.Processing;

public class ServerJobProcessor
{
    private readonly IChannel<Message> _channel;
    private readonly DataStore _dataStore;
    private readonly MessageConverter _messageConverter;
    private readonly string _clientId;
    private readonly ServerCommunicator _communicator;
    private readonly QueryDataHandler _queryHandler;
    private readonly NextDataHandler _nextDataHandler;
    private readonly ServerTerminator _serverTerminator;

    public ServerJobProcessor(IChannel<Message> channel,
        string clientId,
        DataStore dataStore,
        ServerCommunicator communicator,
        QueryDataHandler queryHandler,
        MessageConverter messageConverter,
        NextDataHandler nextDataHandler,
        ServerTerminator serverTerminator)
    {
        _channel = channel;
        _clientId = clientId;
        _dataStore = dataStore;
        _communicator = communicator;
        _queryHandler = queryHandler;
        _messageConverter = messageConverter;
        _nextDataHandler = nextDataHandler;
        _serverTerminator = serverTerminator;
    }

    public async Task ProcessAsync(CancellationToken cancellationToken)
    {
        await foreach (Message message in _channel.ReadAllAsync(cancellationToken))
        {
            var (type, content) = message;

            switch (type)
            {
                case QueryData when content is string recordId:
                    await _queryHandler.HandleAsync(recordId, cancellationToken); 
                    break;
                case NextData when content is NextDataValue nextData:
                    await _nextDataHandler.SaveNextDataAsync(nextData, cancellationToken);
                    break;
                case Unknown:
                    await _communicator.SendBadRequestAsync(
                        new ErrorData($"Bad Request: \"{content}\"."), cancellationToken);
                    break;
                default:
                    await _communicator.SendUnsupportedAsync(
                        new ErrorData($"Unsupported Request: \"{content}\"."), cancellationToken);
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