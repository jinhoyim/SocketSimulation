namespace SocketClientApp.Store;

public class CountStore
{
    private int _successfulCount;
    private int _failedCount;
    
    // Retry 횟수가 일반 실패 횟수에 포함되지 않도록 구분한다.
    private int _failedLockingCount;

    public int IncrementSuccessful()
    {
        return Interlocked.Increment(ref _successfulCount);
    }

    public int IncrementLockingFailed()
    {
        return Interlocked.Increment(ref _failedLockingCount);
    }

    public int IncrementFailed()
    {
        return Interlocked.Increment(ref _failedCount);
    }

    public (int lockingCount, int emptyCount) GetFailedCounts()
    {
        return (lockingCount: _failedLockingCount, emptyCount: _failedCount);
    }
}