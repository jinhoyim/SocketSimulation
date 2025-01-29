using System.Diagnostics.CodeAnalysis;
using SocketCommunicationLib.Model;
using SocketServerApp.Output;

namespace SocketServerApp.Store;

public class SaveLoggingDataStore : IDataStore
{
    private readonly IDataStore _store;
    private readonly OutputWriter _writer;

    public SaveLoggingDataStore(IDataStore store, OutputWriter writer)
    {
        _store = store;
        _writer = writer;
    }

    public void Update(DataRecord record)
    {
        _store.Update(record);
        _writer.WriteRecordUpdated(record);
    }

    public DataRecord InitialDataRecord(TimeSpan initLockTime)
    {
        var record = _store.InitialDataRecord(initLockTime);
        _writer.WriteRecordInit(record);
        return record;
    }

    public bool TryGet(string id, [MaybeNullWhen(false)] out DataRecord record)
    {
        return _store.TryGet(id, out record);
    }

    public bool TryRemove(DataRecord item)
    {
        return _store.TryRemove(item);
    }

    public bool TryCreateNext(string clientId, [MaybeNullWhen(false)] out DataRecord nextRecord)
    {
        if (_store.TryCreateNext(clientId, out nextRecord))
        {
            _writer.WriteRecordNextCreated(nextRecord);
            return true;
        }
        return false;
    }

    public bool IsSaveRemoveCompleted => _store.IsSaveRemoveCompleted;
}