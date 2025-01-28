using SocketCommunicationLib.Contract;
using SocketCommunicationLib.Model;
using SocketServerApp.Communication;
using SocketServerApp.Store;

namespace SocketServerApp.Processing;

public class QueryDataHandler
{
    private readonly string _clientId;
    private readonly ServerCommunicator _communicator;
    private readonly DataStore _store;

    public QueryDataHandler(string clientId, ServerCommunicator communicator, DataStore store)
    {
        _clientId = clientId;
        _communicator = communicator;
        _store = store;
    }
    
    public async Task HandleAsync(string recordId, CancellationToken cancellationToken)
    {    
        if (!_store.TryGet(recordId, out var dataRecord))
        {
            await SendEmptyDataAsync(recordId, cancellationToken);
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
            if (!_store.TryCreateNext(_clientId, out var nextId))
            {
                Console.WriteLine("DataRecord store is full.");
            }
            var recordWithNext = new DataRecordWithNext(dataRecord, nextId);
            await _communicator.SendQueryResultAsync(recordWithNext, cancellationToken);
        }
        else
        {
            await SendEmptyDataAsync(recordId, cancellationToken);
        }
    }

    private async Task SendEmptyDataAsync(string recordId, CancellationToken cancellationToken)
    {
        var errorData = new ErrorData<string>(recordId, $"Data is empty(Id: {recordId})");
        await _communicator.SendEmptyDataAsync(errorData, cancellationToken);
    }
}