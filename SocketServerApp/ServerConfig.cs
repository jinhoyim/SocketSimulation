using System.Net;

public class ServerConfig
{
    public IPEndPoint IpEndPoint { get; }
    public int StartConnectionCount { get; }
    public int EndCount { get; }
    public TimeSpan InitLockTime { get; }
    public int SocketConnectionQueue { get; }
    public TimeSpan ServerTerminatedDelay { get; }
    public int ProcessorCount { get; }

    public ServerConfig(
        string ipAddress,
        int hostPort,
        int startConnectionCount,
        int endCount,
        int initLockTimeSeconds,
        int socketConnectionQueue,
        int serverTerminatedDelaySeconds,
        int processorCount)
    {
        if (!IPAddress.TryParse(ipAddress, out var ip))
        {
            throw new ArgumentException("Invalid IP address.", nameof(ipAddress));
        }

        IpEndPoint = new IPEndPoint(ip, hostPort);
        StartConnectionCount = startConnectionCount;
        EndCount = endCount;
        InitLockTime = TimeSpan.FromSeconds(initLockTimeSeconds);
        SocketConnectionQueue = socketConnectionQueue;
        ServerTerminatedDelay = TimeSpan.FromSeconds(serverTerminatedDelaySeconds);
        ProcessorCount = processorCount;
    }
}