namespace SocketClientApp.Store;

public class DataStore
{
    private int _successfulCount;
    private int _failedCount;
    
    public int IncrementSuccessful()
    {
        return Interlocked.Increment(ref _successfulCount);
    }

    public int IncrementFailed()
    {
        return Interlocked.Increment(ref _failedCount);
    }
}