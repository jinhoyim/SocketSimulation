using System.Collections.Concurrent;
using SocketCommunicationLib.Contract;

namespace SocketClientApp.Store;

public class DataStore
{
    private int _successfulCount;
    private int _failedLockingCount;
    private int _failedEmptyCount;
    private readonly ConcurrentDictionary<string, LockTime> _lockTimes = new();

    public int IncrementSuccessful()
    {
        return Interlocked.Increment(ref _successfulCount);
    }

    public int IncrementLockingFailed()
    {
        return Interlocked.Increment(ref _failedLockingCount);
    }

    public void SaveLockTime(DataRecord record)
    {
        _lockTimes[record.Id] = record.LockTime;
    }

    public LockTime GetLockTime(string id)
    {
        return _lockTimes[id];
    }

    public int IncrementEmptyFailed()
    {
        return Interlocked.Increment(ref _failedEmptyCount);
    }

    public (int lockingCount, int emptyCount) GetFailedCounts()
    {
        return (lockingCount: _failedLockingCount, emptyCount: _failedEmptyCount);
    }
}