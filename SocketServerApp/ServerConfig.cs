using System.Net;
using System.Net.Sockets;

namespace SocketServerApp;

public class ServerConfig
{
    public string IpAddress { get; set; } = "127.0.0.1";
    public int Port { get; set; } = 12345;

    public IPEndPoint IpEndPoint => new(IPAddress.Parse(IpAddress), Port);
    public int StartConnectionCount { get; set; } = 5;
    public int EndCount { get; set; } = 2000;
    public TimeSpan InitLockTime { get; set; } = TimeSpan.FromSeconds(2);
    public int SocketConnectionQueue { get; set; } = 1000;
    public TimeSpan ServerTerminatedDelay { get; set; } = TimeSpan.FromSeconds(5);
    public int ProcessorCount { get; set; } = 3;
    public LingerOption LingerOption { get; set; } = new(true, 10);
}