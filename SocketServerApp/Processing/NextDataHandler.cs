using SocketCommunicationLib.Model;
using SocketServerApp.Communication;
using SocketServerApp.Store;

namespace SocketServerApp.Processing;

public class NextDataHandler
{
    private readonly DataStore _store;
    private readonly string _clientId;
    private readonly ServerCommunicator _communicator;
    private readonly SocketsCommunicator _multiCommunicator;

    public NextDataHandler(
        DataStore store,
        string clientId,
        ServerCommunicator communicator,
        SocketsCommunicator multiCommunicator)
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
            await _communicator.SendNotFoundDataAsync(id, cancellationToken);
            return;
        }

        if (!HasModifyPermission(record))
        {
            await _communicator.SendHasNotModifyPermissionAsync(id, cancellationToken);
            return;
        }

        var updatedRecord = UpdateRecord(record, lockTime, value);
        await _multiCommunicator.SendNextLockTimeAsync(_clientId, updatedRecord, cancellationToken);
    }

    private bool HasModifyPermission(DataRecord record)
    {
        return string.IsNullOrEmpty(record.CreatedClientId) || record.CreatedClientId == _clientId;
    }

    private DataRecord UpdateRecord(DataRecord record, LockTime lockTime, int value)
    {
        var updatedRecord = record.CopyWith(lockTime, value);
        _store.Update(updatedRecord);
        return updatedRecord;
    }
}