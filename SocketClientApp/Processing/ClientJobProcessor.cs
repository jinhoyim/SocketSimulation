using SocketCommunicationLib.Channel;
using SocketCommunicationLib.Contract;
using SocketCommunicationLib.Model;
using static SocketCommunicationLib.Contract.DataProtocolConstants;

namespace SocketClientApp.Processing;

public class ClientJobProcessor
{
    private readonly CancellationTokenSource _cts;
    private readonly QuerySuccessfulHandler _querySuccessfulHandler;
    private readonly QueryHandler _queryHandler;
    private readonly ErrorHandler _errorHandler;

    public ClientJobProcessor(
        QuerySuccessfulHandler querySuccessfulHandler,
        QueryHandler queryHandler,
        ErrorHandler errorHandler,
        CancellationTokenSource cts)
    {
        _cts = cts;
        _querySuccessfulHandler = querySuccessfulHandler;
        _queryHandler = queryHandler;
        _errorHandler = errorHandler;
    }

    public async Task ProcessAsync(IChannel<Message> inputChannel, CancellationToken cancellationToken)
    {
        await foreach (Message message in inputChannel.ReadAllAsync(cancellationToken))
        {
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
                case ErrorNotFoundData when content is ErrorData errorData:
                    _errorHandler.WriteError(errorData.Message);
                    break;
                case ErrorNotModifyPermission when content is ErrorData errorData:
                    _errorHandler.WriteError(errorData.Message);
                    break;
                case ErrorBadRequest when content is ErrorData errorData:
                    _errorHandler.WriteError(errorData.Message);
                    break;
                case ErrorUnsupportedRequest when content is ErrorData errorData:
                    _errorHandler.WriteError(errorData.Message);
                    break;
                case Unknown:
                    _errorHandler.WriteError($"Unknown message. {content}");
                    break;
                case ProtocolConstants.ServerTerminated:
                    await _cts.CancelAsync();
                    break;
            }
        }
    }
}