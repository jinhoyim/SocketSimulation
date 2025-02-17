using System.Net.Sockets;
using SocketCommunicationLib;
using SocketCommunicationLib.Contract;
using SocketCommunicationLib.Model;

namespace SocketServerApp.Communication;

public class ClientCommunicator(ClientSession session) : SocketCommunicator(session.Socket)
{
    public string ClientId => session.ClientId;
    
    public async Task SendEmptyDataAsync(ErrorData<string> data, CancellationToken cancellationToken)
    {
        await SendAsync(data, DataProtocolConstants.ErrorEmptyData, cancellationToken);
    }

    public async Task SendDataLockedAsync(ErrorData<string> data, CancellationToken cancellationToken)
    {
        await SendAsync(data, DataProtocolConstants.ErrorDataLocked, cancellationToken);
    }

    public async Task SendNotFoundDataAsync(ErrorData errorData, CancellationToken cancellationToken)
    {
        await SendAsync(errorData, DataProtocolConstants.ErrorNotFoundData, cancellationToken);
    }

    public async Task SendHasNotModifyPermissionAsync(ErrorData errorData, CancellationToken cancellationToken)
    {
        await SendAsync(errorData, DataProtocolConstants.ErrorNotModifyPermission, cancellationToken);
    }
    
    public async Task SendBadRequestAsync(ErrorData errorData, CancellationToken cancellationToken)
    {
        await SendAsync(errorData, DataProtocolConstants.ErrorBadRequest, cancellationToken);
    }

    public async Task SendUnsupportedAsync(ErrorData errorData, CancellationToken cancellationToken)
    {
        await SendAsync(errorData, DataProtocolConstants.ErrorUnsupportedRequest, cancellationToken);
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