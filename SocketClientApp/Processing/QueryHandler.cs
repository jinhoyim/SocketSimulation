using SocketClientApp.Communication;
using SocketClientApp.Store;
using SocketCommunicationLib.Model;

namespace SocketClientApp.Processing;

public class QueryHandler
{
    private readonly SocketCommunicator _communicator;
    private readonly LockTimesStore _lockTimesStore;

    public QueryHandler(SocketCommunicator communicator, LockTimesStore lockTimesStore)
    {
        _communicator = communicator;
        _lockTimesStore = lockTimesStore;
    }

    public async Task QueryAfterLockTimeAsync(DataRecord dataRecord, CancellationToken cancellationToken)
    {
        _lockTimesStore.SaveLockTime(dataRecord.Id, dataRecord.LockTime);
        
        await WaitLockTimeAsync(dataRecord.LockTime, cancellationToken);
        await _communicator.SendQueryAsync(dataRecord.Id, cancellationToken);
    }

    public async Task RetryQueryAfterLockTimeAsync(string id, CancellationToken cancellationToken)
    {
        if (!_lockTimesStore.TryGetLockTime(id, out var lockTime)) return;
        
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