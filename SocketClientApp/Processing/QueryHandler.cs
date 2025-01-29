using SocketClientApp.Communication;
using SocketClientApp.Store;
using SocketCommunicationLib.Model;

namespace SocketClientApp.Processing;

public class QueryHandler
{
    private readonly ClientCommunicator _communicator;
    private readonly LockTimesStore _lockTimesStore;
    private readonly bool _afterLockTime;

    public QueryHandler(ClientCommunicator communicator, LockTimesStore lockTimesStore, bool afterLockTime)
    {
        _communicator = communicator;
        _lockTimesStore = lockTimesStore;
        _afterLockTime = afterLockTime;
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
        if (_afterLockTime)
        {
            delay = delay.Add(TimeSpan.FromMilliseconds(1));
        }

        if (delay > TimeSpan.Zero)
        {
            await Task.Delay(delay, cancellationToken);
        }
    }
}