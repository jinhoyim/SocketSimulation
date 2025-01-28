using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using SocketCommunicationLib.Model;

namespace SocketServerApp.Store;

public class DataStore
{
    private readonly Lock _lock = new Lock();
    private readonly ConcurrentDictionary<string, DataRecord> _dataCache = new();
    private readonly int _maxSize;
    
    private int _saveCount = 0;
    private int _increment = 0;

    public DataStore(int maxSize)
    {
        _maxSize = maxSize;
    }
    
    public int Save(DataRecord record)
    {
        var id = Interlocked.Increment(ref _saveCount);
        _dataCache[id.ToString()] = record;
        return id;
    }

    public void Update(DataRecord record)
    {
        _dataCache[record.Id] = record;
    }

    public DataRecord InitialDataRecord()
    {
        var dateTime = DateTime.Now.AddSeconds(2);
        var lockTime = LockTime.From(dateTime);
        var number = Interlocked.Increment(ref _increment);
        var id = number.ToString();
        return new DataRecord(id, lockTime, string.Empty, dateTime.Millisecond);
    }
    
    public bool TryGet(string id, [MaybeNullWhen(false)] out DataRecord record)
    {
        return _dataCache.TryGetValue(id, out record);
    }

    public bool TryRemove(DataRecord item)
    {
        return _dataCache.TryRemove(KeyValuePair.Create(item.Id, item));
    }

    public bool TryCreateNext(string clientId, [MaybeNullWhen(false)] out string nextId)
    {
        if (_increment >= _maxSize)
        {
            nextId = null;
            return false;
        }
        var number = Interlocked.Increment(ref _increment);
        nextId = number.ToString();
        _dataCache[nextId] = DataRecord.Empty with { Id = nextId, CreatedClientId = clientId };
        return true;
    }
}