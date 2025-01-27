namespace SocketServerApp;

public class DataRecordStore
{
    private int _saveCount = 0;

    public async Task<int> SaveAsync()
    {
        Interlocked.Increment(ref _saveCount);
        return await Task.FromResult(_saveCount);
    }
}