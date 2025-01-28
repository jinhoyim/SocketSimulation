using SocketClientApp.Communication;
using SocketCommunicationLib.Channel;
using SocketCommunicationLib.Contract;
using SocketCommunicationLib.Model;
using static SocketCommunicationLib.Contract.DataProtocolConstants;

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

            var (type, content) = message;

            switch (type)
            {
                case DataLockTime when content is DataRecord record:
                    await _queryHandler.QueryAfterLockTimeAsync(record, cancellationToken);
                    break;
                case DataWithNext when content is DataRecordWithNext withNext:
                    await _querySuccessfulHandler.SaveAndNextAsync(withNext, cancellationToken);
                    break;
                case ErrorEmptyData when content is ErrorData<string> errorData:
                    _errorHandler.WriteErrorEmptyData(errorData.Message, errorData.Data);
                    break;
                case ErrorDataLocked when content is ErrorData<string> errorData:
                    _errorHandler.WriteErrorDataLocked(errorData.Message);
                    await _queryHandler.RetryQueryAfterLockTimeAsync(errorData.Data, cancellationToken);
                    break;
                case ErrorBadRequest:
                    Console.WriteLine($"Error: {ErrorBadRequest}");
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