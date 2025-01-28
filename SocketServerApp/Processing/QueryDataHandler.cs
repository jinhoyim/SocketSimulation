using SocketCommunicationLib.Contract;
using SocketServerApp.Communication;
using SocketServerApp.Store;

namespace SocketServerApp.Processing;

public class QueryDataHandler
{
    private readonly string _clientId;
    private readonly SocketCommunicator _communicator;
    private readonly DataStore _store;

    public QueryDataHandler(string clientId, SocketCommunicator communicator, DataStore store)
    {
        _clientId = clientId;
        _communicator = communicator;
        _store = store;
    }
    
    public async Task HandleAsync(string recordId, CancellationToken cancellationToken)
    {    
        if (!_store.TryGet(recordId, out var dataRecord))
        {
            await _communicator.SendEmptyDataAsync($"Id: {recordId}", cancellationToken);
            return;
        }

        if (!dataRecord.LockTime.IsExpired(DateTime.Now))
        {
            var errorData = new ErrorData<string>(recordId, $"Cannot access locked data(Id: {recordId})");
            await _communicator.SendDataLockedAsync(errorData, cancellationToken);
            return;
        }

        if (_store.TryRemove(dataRecord))
        {
            _store.TryCreateNext(_clientId, out var nextId);
            var recordWithNext = new DataRecordWithNext(dataRecord, nextId);
            await _communicator.SendQueryResultAsync(recordWithNext, cancellationToken);
        }
        else
        {
            await _communicator.SendEmptyDataAsync($"Id: {recordId}", cancellationToken);
        }
    }
}