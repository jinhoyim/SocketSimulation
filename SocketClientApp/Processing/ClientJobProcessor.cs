using SocketClientApp.Communication;
using SocketCommunicationLib.Channel;
using SocketCommunicationLib.Contract;

namespace SocketClientApp.Processing;

public class ClientJobProcessor
{
    private readonly IChannel<string> _channel;
    private readonly SocketCommunicator _communicator;
    private readonly CancellationTokenSource _cts;
    private readonly MessageConverter _messageConverter;
    private readonly QueryResultHandler _queryResultHandler;
    private readonly LockTimeHandler _lockTimeHandler;

    public ClientJobProcessor(
        IChannel<string> channel,
        SocketCommunicator communicator,
        MessageConverter messageConverter,
        QueryResultHandler queryResultHandler,
        LockTimeHandler lockTimeHandler,
        CancellationTokenSource cts)
    {
        _channel = channel;
        _communicator = communicator;
        _cts = cts;
        _messageConverter = messageConverter;
        _queryResultHandler = queryResultHandler;
        _lockTimeHandler = lockTimeHandler;
    }

    public async Task ProcessAsync(CancellationToken cancellationToken)
    {
        await foreach (var request in _channel.ReadAllAsync(cancellationToken))
        {
            var message = _messageConverter.Convert(request);
            if (message == Message.Empty) continue;
            
            switch (message.Type)
            {
                case ProtocolConstants.LockTime:
                    await _lockTimeHandler.HandleLockTimeAsync(message.Content, cancellationToken);
                    break;
                case ProtocolConstants.DataRecordWithNext:
                    _queryResultHandler.Handle(message.Content);
                    break;
                case ProtocolConstants.ErrorEmptyData:
                    Console.WriteLine($"Error: {ProtocolConstants.ErrorEmptyData}");
                    break;
                case ProtocolConstants.ErrorDataLocked:
                    Console.WriteLine($"Error: {ProtocolConstants.ErrorDataLocked}");
                    break;
                case ProtocolConstants.ErrorBadRequest:
                    Console.WriteLine($"Error: {ProtocolConstants.ErrorBadRequest}");
                    break;
                case ProtocolConstants.ServerTerminated:
                    await _cts.CancelAsync();
                    break;
                
            // else if ReceiveData
            // Logging Data + LockTime + SuccessCount
            // Create Data + LockTime
            
            // else if NotYetExpiredLockTime
            // Logging NotYetExpiredLockTime Count
            // delay LockTime
            // retry query QueryData
            
            // else if DataIsEmpty
            // Logging FailedCount
                
            }
        }
    }
}