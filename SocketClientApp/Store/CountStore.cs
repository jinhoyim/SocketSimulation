namespace SocketClientApp.Store;

public class CountStore
{
    private int _successfulCount;
    private int _failedLockingCount;
    private int _failedEmptyCount;

    public int IncrementSuccessful()
    {
        return Interlocked.Increment(ref _successfulCount);
    }

    public int IncrementLockingFailed()
    {
        return Interlocked.Increment(ref _failedLockingCount);
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