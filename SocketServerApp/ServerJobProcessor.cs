using SocketCommunicationLib;

namespace SocketServerApp;

public class ServerJobProcessor
{
    private readonly IChannel<string> _channel;
    private readonly CancellationTokenSource _cts;
    private readonly DataRecordStore _dataRecordStore;
    private readonly SocketsCommunicator _socketsCommunicator;

    public ServerJobProcessor(
        IChannel<string> channel,
        CancellationTokenSource cts,
        DataRecordStore dataRecordStore,
        SocketsCommunicator socketsCommunicator)
    {
        _channel = channel;
        _cts = cts;
        _dataRecordStore = dataRecordStore;
        _socketsCommunicator = socketsCommunicator;
    }

    public async Task ProcessAsync(CancellationToken cancellationToken)
    {
        await foreach (var item in _channel.ReadAllAsync(cancellationToken))
        {
            Console.WriteLine(item);

            var savedCount = await _dataRecordStore.SaveAsync();
            if (savedCount == 4)
            {
                await ServerTerminate(cancellationToken);
            }
        }
    }

    private async Task ServerTerminate(CancellationToken cancellationToken)
    {
        await _socketsCommunicator.SendServerTerminateAsync(cancellationToken);
        await Task.Delay(TimeSpan.FromSeconds(3), cancellationToken);
        await _cts.CancelAsync();
    }
}