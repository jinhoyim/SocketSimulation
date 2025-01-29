using SocketClientApp.Output;
using SocketClientApp.Store;

namespace SocketClientApp.Processing;

public class ErrorHandler
{
    private readonly CountStore _countStore;
    private readonly OutputWriter _writer;
    private readonly LockTimesStore _lockTimesStore;

    public ErrorHandler(CountStore countStore, OutputWriter writer, LockTimesStore lockTimesStore)
    {
        _countStore = countStore;
        _writer = writer;
        _lockTimesStore = lockTimesStore;
    }

    public void WriteErrorDataLocked(string errorMessage)
    {
        _countStore.IncrementLockingFailed();
        _writer.WriteError(errorMessage);
    }

    public void WriteErrorEmptyData(string errorMessage, string recordId)
    {
        _countStore.IncrementFailed();
        _writer.WriteError(errorMessage);
        _lockTimesStore.TryRemoveLockTime(recordId);
    }
    
    public void WriteError(string errorMessage)
    {
        _countStore.IncrementFailed();
        _writer.WriteError(errorMessage);
    }
}