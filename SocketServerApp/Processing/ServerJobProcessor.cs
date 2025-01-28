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
    private readonly DataRecordStore _dataRecordStore;
    private readonly SocketsCommunicator _socketsCommunicator;
    private readonly MessageConverter _messageConverter;
    private readonly SocketCommunicator _communicator;
    private readonly string _clientId;
    private readonly QueryDataHandler _queryHandler;

    public ServerJobProcessor(
        IChannel<string> channel,
        string clientId,
        SocketCommunicator communicator,
        DataRecordStore dataRecordStore,
        SocketsCommunicator socketsCommunicator,
        QueryDataHandler queryHandler,
        MessageConverter messageConverter,
        CancellationTokenSource cts)
    {
        _channel = channel;
        _clientId = clientId;
        _communicator = communicator;
        _dataRecordStore = dataRecordStore;
        _socketsCommunicator = socketsCommunicator;
        _queryHandler = queryHandler;
        _messageConverter = messageConverter;
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
            }

            // var savedCount = _dataRecordStore.Save();
            // if (savedCount == 4)
            // {
            await ServerTerminate(cancellationToken);
            // }
        }
    }

    private async Task ServerTerminate(CancellationToken cancellationToken)
    {
        // await _socketsCommunicator.SendServerTerminateAsync(cancellationToken);
        // await Task.Delay(TimeSpan.FromSeconds(3), cancellationToken);
        // await _cts.CancelAsync();
    }
}