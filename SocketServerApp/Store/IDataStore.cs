using System.Diagnostics.CodeAnalysis;
using SocketCommunicationLib.Model;

namespace SocketServerApp.Store;

public interface IDataStore
{
    void Update(DataRecord record);
    DataRecord InitialDataRecord(TimeSpan initLockTime);
    bool TryGet(string id, [MaybeNullWhen(false)] out DataRecord record);
    bool TryRemove(DataRecord item);
    bool TryCreateNext(string clientId, [MaybeNullWhen(false)] out DataRecord nextRecord);
    bool IsSaveRemoveCompleted { get; }
}