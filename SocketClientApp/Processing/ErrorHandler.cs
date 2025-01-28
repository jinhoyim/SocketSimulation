using SocketClientApp.Output;
using SocketClientApp.Store;
using SocketCommunicationLib;
using SocketCommunicationLib.Contract;

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

    public void WriteErrorDataLocked(string content)
    {
        _countStore.IncrementLockingFailed();
        
        ErrorData<string>? errorData = JsonUtils.Deserialize<ErrorData<string>>(content);
        if (errorData is null) return;
        
        WriteError(errorData);
    }

    public void WriteErrorEmptyData(string content)
    {
        _countStore.IncrementEmptyFailed();

        ErrorData<string>? errorData = JsonUtils.Deserialize<ErrorData<string>>(content);
        if (errorData is null) return;
        
        WriteError(errorData);
        _lockTimesStore.TryRemoveLockTime(errorData.Data);
    }

    private void WriteError(ErrorData<string> errorData)
    {
        var (lockingCount, emptyCount) = _countStore.GetFailedCounts();
        _writer.WriteError(errorData.Message, lockingCount, emptyCount);
    }
}