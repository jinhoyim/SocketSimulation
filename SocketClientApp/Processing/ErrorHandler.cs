using SocketClientApp.Output;
using SocketClientApp.Store;
using SocketCommunicationLib;
using SocketCommunicationLib.Contract;

namespace SocketClientApp.Processing;

public class ErrorHandler
{
    private readonly DataStore _store;
    private readonly OutputWriter _writer;

    public ErrorHandler(DataStore store, OutputWriter writer)
    {
        _store = store;
        _writer = writer;
    }

    public void WriteErrorDataLocked(string content)
    {
        var failedCount = _store.IncrementLockingFailed();
        
        ErrorData<string>? errorData = JsonUtils.Deserialize<ErrorData<string>>(content);
        if (errorData is null) return;
        
        _writer.WriteError(errorData.Message, failedCount);
    }
}