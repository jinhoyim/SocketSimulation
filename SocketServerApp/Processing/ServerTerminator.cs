using Microsoft.Extensions.Options;
using SocketServerApp.Communication;

namespace SocketServerApp.Processing;

public class ServerTerminator
{
    private readonly AllCilentsCommunicator _communicator;
    private readonly TimeSpan _serverTerminatedDelay;
    private readonly CancellationTokenSource _cts;
    private bool _terminated = false;
    private readonly Lock _terminationLock = new();

    public ServerTerminator(
        AllCilentsCommunicator communicator,
        IOptions<ServerConfig> serverConfig,
        CancellationTokenSource cts)
    {
        _communicator = communicator;
        _serverTerminatedDelay = serverConfig.Value.ServerTerminatedDelay;
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