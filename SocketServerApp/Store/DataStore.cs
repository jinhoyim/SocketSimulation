using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Options;
using SocketCommunicationLib.Model;

namespace SocketServerApp.Store;

public class DataStore : IDataStore
{
    private readonly ConcurrentDictionary<string, DataRecord> _dataCache = new();
    private readonly int _maxSize;
    
    // save cound
    private int _saveCount = 0;

    // remove count
    private int _removeCount = 0;
    
    // id
    private int _increment = 0;

    public DataStore(IOptions<ServerConfig> config)
    {
        _maxSize = config.Value.EndCount;
    }

    public void Update(DataRecord record)
    {
        _dataCache[record.Id] = record;
        Interlocked.Increment(ref _saveCount);
    }

    public DataRecord InitialDataRecord(TimeSpan initLockTime)
    {
        var dateTime = DateTime.Now.Add(initLockTime);
        var lockTime = LockTime.From(dateTime);
        var number = Interlocked.Increment(ref _increment);
        var id = number.ToString();
        var dataRecord = new DataRecord(id, lockTime, string.Empty, dateTime.Millisecond);
        Interlocked.Increment(ref _saveCount);
        _dataCache[id] = dataRecord;
        return dataRecord;
    }
    
    public bool TryGet(string id, [MaybeNullWhen(false)] out DataRecord record)
    {
        return _dataCache.TryGetValue(id, out record);
    }

    public bool TryRemove(DataRecord item)
    {
        if (_dataCache.TryRemove(KeyValuePair.Create(item.Id, item)))
        {
            Interlocked.Increment(ref _removeCount);
            return true;
        }
        return false;
    }

    public bool TryCreateNext(string clientId, [MaybeNullWhen(false)] out DataRecord nextRecord)
    {
        if (_increment >= _maxSize)
        {
            nextRecord = null;
            return false;
        }
        var number = Interlocked.Increment(ref _increment);
        string nextId = number.ToString();
        nextRecord = DataRecord.Create(nextId, clientId);
        _dataCache[nextId] = nextRecord;
        return true;
    }

    public bool IsSaveRemoveCompleted => _saveCount >= _maxSize && _removeCount >= _maxSize;
}