using SocketCommunicationLib;
using SocketCommunicationLib.Contract;
using SocketServerApp.Communication;
using SocketServerApp.Store;
using static SocketCommunicationLib.Contract.ProtocolConstants;

namespace SocketServerApp.Processing;

public class QueryDataHandler
{
    private readonly string _clientId;
    private readonly SocketCommunicator _communicator;
    private readonly DataRecordStore _store;

    public QueryDataHandler(string clientId, SocketCommunicator communicator, DataRecordStore store)
    {
        _clientId = clientId;
        _communicator = communicator;
        _store = store;
    }
    
    public async Task HandleAsync(string content, CancellationToken cancellationToken)
    {
        DataRecord? body = JsonUtils.Deserialize<DataRecord>(content);

        if (body is null)
        {
            await _communicator.SendBadRequestAsync(QueryData, cancellationToken);
            return;
        }

        string recordId = body.Id;
            
        if (!_store.TryGet(recordId, out var dataRecord))
        {
            await _communicator.SendEmptyDataAsync($"Id: {recordId}", cancellationToken);
            return;
        }

        if (!dataRecord.LockTime.IsExpired(DateTime.Now))
        {
            await _communicator.SendDataLockedAsync($"Id: {recordId}", cancellationToken);
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