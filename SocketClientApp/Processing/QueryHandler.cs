using SocketClientApp.Communication;
using SocketCommunicationLib;
using SocketCommunicationLib.Contract;

namespace SocketClientApp.Processing;

public class QueryHandler
{
    private readonly SocketCommunicator _communicator;

    public QueryHandler(SocketCommunicator communicator)
    {
        _communicator = communicator;
    }

    public async Task QueryAfterLockTimeAsync(string content, CancellationToken cancellationToken)
    {
        DataRecord? dataRecord = JsonUtils.Deserialize<DataRecord>(content);
        if (dataRecord is not null)
        {
            await WaitLockTimeAsync(dataRecord.LockTime, cancellationToken);
            await _communicator.SendQueryAsync(dataRecord, cancellationToken);
        }
    }
    
    private async Task WaitLockTimeAsync(LockTime lockTime, CancellationToken cancellationToken)
    {
        var delay = lockTime.TimeLeftToExpire(DateTime.Now);
        if (delay > TimeSpan.Zero)
        {
            await Task.Delay(delay, cancellationToken);
        }
    }
}