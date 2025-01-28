using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using SocketCommunicationLib.Contract;

namespace SocketClientApp.Store;

public class LockTimesStore
{
    private readonly ConcurrentDictionary<string, LockTime> _lockTimes = new();

    public void SaveLockTime(string id, LockTime lockTime)
    {
        _lockTimes[id] = lockTime;
    }

    public bool TryGetLockTime(string id, [MaybeNullWhen(false)] out LockTime record)
    {
        return _lockTimes.TryGetValue(id, out record);
    }

    public bool TryRemoveLockTime(string id)
    {
        return _lockTimes.TryRemove(id, out _);
    }
}