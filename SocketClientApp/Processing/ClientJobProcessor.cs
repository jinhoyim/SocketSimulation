using SocketClientApp.Communication;
using SocketCommunicationLib.Channel;
using SocketCommunicationLib.Contract;

namespace SocketClientApp.Processing;

public class ClientJobProcessor
{
    private readonly IChannel<Message> _channel;
    private readonly SocketCommunicator _communicator;
    private readonly CancellationTokenSource _cts;
    private readonly MessageConverter _messageConverter;
    private readonly QuerySuccessfulHandler _querySuccessfulHandler;
    private readonly QueryHandler _queryHandler;
    private readonly ErrorHandler _errorHandler;

    public ClientJobProcessor(
        IChannel<Message> channel,
        SocketCommunicator communicator,
        MessageConverter messageConverter,
        QuerySuccessfulHandler querySuccessfulHandler,
        QueryHandler queryHandler,
        ErrorHandler errorHandler,
        CancellationTokenSource cts)
    {
        _channel = channel;
        _communicator = communicator;
        _cts = cts;
        _messageConverter = messageConverter;
        _querySuccessfulHandler = querySuccessfulHandler;
        _queryHandler = queryHandler;
        _errorHandler = errorHandler;
    }

    public async Task ProcessAsync(CancellationToken cancellationToken)
    {
        await foreach (Message message in _channel.ReadAllAsync(cancellationToken))
        {
            if (message == Message.Empty) continue;

            switch (message.Type)
            {
                case ProtocolConstants.LockTime:
                    await _queryHandler.QueryAfterLockTimeAsync(message.Content, cancellationToken);
                    break;
                case ProtocolConstants.DataRecordWithNext:
                    await _querySuccessfulHandler.SaveAndNextAsync(message.Content, cancellationToken);
                    break;
                case ProtocolConstants.ErrorEmptyData:
                    _errorHandler.WriteErrorEmptyData(message.Content);
                    break;
                case ProtocolConstants.ErrorDataLocked:
                    _errorHandler.WriteErrorDataLocked(message.Content);
                    await _queryHandler.RetryQueryAfterLockTimeAsync(message.Content, cancellationToken);
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