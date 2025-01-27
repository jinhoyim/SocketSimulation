using SocketCommunicationLib;

namespace SocketClientApp;

public class ClientJobProcessor
{
    private readonly IChannel<string> _channel;
    private readonly SocketCommunicator _communicator;
    private readonly CancellationTokenSource _cts;

    public ClientJobProcessor(IChannel<string> channel, SocketCommunicator communicator, CancellationTokenSource cts)
    {
        _channel = channel;
        _communicator = communicator;
        _cts = cts;
    }

    public async Task ProcessAsync(CancellationToken cancellationToken)
    {
        int count = 0;
        await foreach (var item in _channel.ReadAllAsync(cancellationToken))
        {
            
            Console.WriteLine($"process: {item}{count}");

            if (item.StartsWith(ProtocolConstants.ServerTerminated))
            {
                await _cts.CancelAsync();
            }
            else
            {
                await _communicator.SendRecordAsync(item, cancellationToken);
            }

            count++;
            // Convert Message
            
            // if LockTime
            // delay LockTime
            // query QueryData
            
            // else if ReceiveData
            // Logging Data + LockTime + SuccessCount
            // Create Data + LockTime
            
            // else if NotYetExpiredLockTime
            // Logging NotYetExpiredLockTime Count
            // delay LockTime
            // retry query QueryData
            
            // else if DataIsEmpty
            // Logging FailedCount
        }
    }
}