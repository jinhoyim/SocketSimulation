using SocketCommunicationLib.Contract;
using SocketCommunicationLib.Model;
using SocketServerApp.Communication;
using SocketServerApp.Store;

namespace SocketServerApp.Processing;

public class NextDataHandler
{
    private readonly IDataStore _store;
    private readonly string _clientId;
    private readonly ClientCommunicator _communicator;
    private readonly AllCilentsCommunicator _multiCommunicator;

    public NextDataHandler(
        IDataStore store,
        string clientId,
        ClientCommunicator communicator,
        AllCilentsCommunicator multiCommunicator)
    {
        _store = store;
        _clientId = clientId;
        _communicator = communicator;
        _multiCommunicator = multiCommunicator;
    }

    public async Task SaveNextDataAsync(NextDataValue nextData, CancellationToken cancellationToken)
    {
        var (id, lockTime, value) = nextData;
        
        if (!_store.TryGet(id, out var record))
        {
            var errorData = new ErrorData($"Id: {id}, Could not find record.");
            await _communicator.SendNotFoundDataAsync(errorData, cancellationToken);
            return;
        }

        if (record.TryUpdate(_clientId, lockTime, value, out var updatedRecord, out var errorMessage))
        {
            _store.Update(updatedRecord);    
            await _multiCommunicator.SendNextLockTimeAsync(_clientId, updatedRecord, cancellationToken);
        }
        else
        {
            var errorData = new ErrorData(errorMessage);
            await _communicator.SendHasNotModifyPermissionAsync(errorData, cancellationToken);
        }
    }
}