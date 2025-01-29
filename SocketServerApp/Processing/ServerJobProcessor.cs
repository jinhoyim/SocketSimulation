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
    private readonly ServerCommunicator _communicator;
    private readonly QueryDataHandler _queryHandler;
    private readonly NextDataHandler _nextDataHandler;
    private readonly ServerTerminator _serverTerminator;

    public ServerJobProcessor(IChannel<Message> channel,
        DataStore dataStore,
        ServerCommunicator communicator,
        QueryDataHandler queryHandler,
        NextDataHandler nextDataHandler,
        ServerTerminator serverTerminator)
    {
        _channel = channel;
        _dataStore = dataStore;
        _communicator = communicator;
        _queryHandler = queryHandler;
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

            if (_dataStore.IsSaveRemoveCompleted)
            {
                await _serverTerminator.ServerTerminate(cancellationToken);
            }
        }
    }
}