using System.Net.Sockets;
using SocketCommunicationLib;
using SocketCommunicationLib.Contract;
using SocketCommunicationLib.Model;

namespace SocketServerApp.Communication;

public class ServerCommunicator(Socket socket) : SocketCommunicator(socket)
{
    public async Task SendEmptyDataAsync(ErrorData<string> data, CancellationToken cancellationToken)
    {
        await SendAsync(data, DataProtocolConstants.ErrorEmptyData, cancellationToken);
    }

    public async Task SendDataLockedAsync(ErrorData<string> data, CancellationToken cancellationToken)
    {
        await SendAsync(data, DataProtocolConstants.ErrorDataLocked, cancellationToken);
    }

    public async Task SendNotFoundDataAsync(string id, CancellationToken cancellationToken)
    {
        var errorData = new ErrorData<string>(id, $"Could not find record with id '{id}'");
        await SendAsync(errorData, DataProtocolConstants.ErrorNotFoundData, cancellationToken);
    }

    public async Task SendHasNotModifyPermissionAsync(string id, CancellationToken cancellationToken)
    {
        var errorData = new ErrorData<string>(id, $"Has not modify permission for record with id '{id}'");
        await SendAsync(errorData, DataProtocolConstants.ErrorNotModifyPermission, cancellationToken);
    }
    
    public async Task SendBadRequestAsync(string content, CancellationToken cancellationToken)
    {
        var errorMessage = $"Bad Request: \"{content}\".";
        var errorData = new ErrorData(errorMessage);
        await SendAsync(errorData, DataProtocolConstants.ErrorBadRequest, cancellationToken);
    }

    public async Task SendUnsupportedAsync(string content, CancellationToken cancellationToken)
    {
        var errorMessage = $"Unsupported Request: \"{content}\".";
        var errorData = new ErrorData(errorMessage);
        await SendAsync(errorData, DataProtocolConstants.UnsupportedRequest, cancellationToken);
    }

    public async Task SendLockTimeAsync(DataRecord data, CancellationToken cancellationToken)
    {
        await SendAsync(data, DataProtocolConstants.DataLockTime, cancellationToken);
    }

    public async Task SendQueryResultAsync(DataRecordWithNext data, CancellationToken cancellationToken)
    {
        await SendAsync(data, DataProtocolConstants.DataWithNext, cancellationToken);
    }
}