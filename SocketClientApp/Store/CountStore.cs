namespace SocketClientApp.Store;

public class CountStore
{
    private int _successfulCount;
    private int _failedCount;
    
    // Retry 횟수가 일반 실패 횟수에 포함되지 않도록 구분한다.
    private int _failedLockingCount;

    public void IncrementSuccessful()
    {
        Interlocked.Increment(ref _successfulCount);
    }

    public void IncrementLockingFailed()
    {
        Interlocked.Increment(ref _failedLockingCount);
    }

    public void IncrementFailed()
    {
        Interlocked.Increment(ref _failedCount);
    }

    public override string ToString()
    {
        return $"Success: {_successfulCount}, Failed: {_failedCount}, Locking: {_failedLockingCount}";
    }
}