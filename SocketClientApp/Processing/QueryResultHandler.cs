using SocketClientApp.Communication;
using SocketClientApp.Output;
using SocketClientApp.Store;
using SocketCommunicationLib;
using SocketCommunicationLib.Contract;

namespace SocketClientApp.Processing;

public class QueryResultHandler
{
    private readonly DataStore _store;
    private readonly OutputWriter _writer;
    private readonly int _maxMilliseconds = 2000;

    public QueryResultHandler(DataStore store, OutputWriter writer)
    {
        _store = store;
        _writer = writer;
    }

    public void Handle(string content)
    {
        var withNext = JsonUtils.Deserialize<DataRecordWithNext>(content);
        if (withNext is null) return;

        var successful = _store.IncrementSuccessful();
        
        _writer.Write(withNext.DataRecord, successful);
    }
}