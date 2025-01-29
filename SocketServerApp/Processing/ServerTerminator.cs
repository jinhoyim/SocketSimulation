using SocketServerApp.Communication;

namespace SocketServerApp.Processing;

public class ServerTerminator
{
    private readonly SocketsCommunicator _communicator;
    private readonly TimeSpan _serverTerminatedDelay;
    private readonly CancellationTokenSource _cts;
    private bool _terminated = false;
    private readonly Lock _terminationLock = new();

    public ServerTerminator(
        SocketsCommunicator communicator,
        TimeSpan serverTerminatedDelay,
        CancellationTokenSource cts)
    {
        _communicator = communicator;
        _serverTerminatedDelay = serverTerminatedDelay;
        _cts = cts;
    }

    public async Task ServerTerminate(CancellationToken cancellationToken)
    {
        bool shouldTerminate = false;
        lock (_terminationLock)
        {
            if (!_terminated)
            {
                _terminated = true;
                shouldTerminate = true;
            }
        }
        
        if (!shouldTerminate) return;
        
        await _communicator.SendServerTerminateAsync(cancellationToken);
        await Task.Delay(_serverTerminatedDelay, cancellationToken);
        await _cts.CancelAsync();
    }
}