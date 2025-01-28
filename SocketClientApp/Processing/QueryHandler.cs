using SocketClientApp.Communication;
using SocketClientApp.Store;
using SocketCommunicationLib;
using SocketCommunicationLib.Contract;

namespace SocketClientApp.Processing;

public class QueryHandler
{
    private readonly SocketCommunicator _communicator;
    private readonly DataStore _store;

    public QueryHandler(SocketCommunicator communicator, DataStore store)
    {
        _communicator = communicator;
        _store = store;
    }

    public async Task QueryAfterLockTimeAsync(string content, CancellationToken cancellationToken)
    {
        DataRecord? dataRecord = JsonUtils.Deserialize<DataRecord>(content);
        if (dataRecord is null) return;

        _store.SaveLockTime(dataRecord);
        
        await WaitLockTimeAsync(dataRecord.LockTime, cancellationToken);
        await _communicator.SendQueryAsync(dataRecord.Id, cancellationToken);
    }

    public async Task RetryQueryAfterLockTimeAsync(string content, CancellationToken cancellationToken)
    {
        var errorData = JsonUtils.Deserialize<ErrorData<string>>(content);
        if (errorData is null) return;

        var id = errorData.Data;
        var lockTime = _store.GetLockTime(id);
        
        await WaitLockTimeAsync(lockTime, cancellationToken);
        await _communicator.SendQueryAsync(id, cancellationToken);
    }
    
    private async Task WaitLockTimeAsync(LockTime lockTime, CancellationToken cancellationToken)
    {
        var delay = lockTime.TimeLeftToExpire(DateTime.Now);
        if (delay > TimeSpan.Zero)
        {
            await Task.Delay(delay, cancellationToken);
        }
    }
}