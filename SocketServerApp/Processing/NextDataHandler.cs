using SocketCommunicationLib.Model;
using SocketServerApp.Communication;
using SocketServerApp.Store;

namespace SocketServerApp.Processing;

public class NextDataHandler
{
    private readonly DataStore _store;
    private readonly string _clientId;
    private readonly SocketsCommunicator _multiCommunicator;

    public NextDataHandler(DataStore store, string clientId, SocketsCommunicator multiCommunicator)
    {
        _store = store;
        _clientId = clientId;
        _multiCommunicator = multiCommunicator;
    }

    public async Task SaveNextDataAsync(NextDataValue nextData, CancellationToken cancellationToken)
    {
        var (id, lockTime, value) = nextData;
        
        if (!_store.TryGet(id, out var record))
        {
            // 서버 오류 데이터를 추가할 준비가 되지 않음 오류
            return;
        }

        if (record.CreatedClientId != _clientId && !string.IsNullOrEmpty(record.CreatedClientId))
        {
            // 저장할 수 있는 Client 가 아니다.
        }

        var updatedRecord = UpdateRecord(record, lockTime, value);
        await _multiCommunicator.SendNextLockTimeAsync(_clientId, updatedRecord, cancellationToken);
    }

    private DataRecord UpdateRecord(DataRecord record, LockTime lockTime, int value)
    {
        var updatedRecord = record.CopyWith(lockTime, value);
        _store.Update(updatedRecord);
        return updatedRecord;
    }
}