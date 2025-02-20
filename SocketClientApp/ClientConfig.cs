using System.Net;
using System.Net.Sockets;

namespace SocketClientApp;

public class ClientConfig
{
    public string ClientId { get; set; } = string.Empty;
    public string IpAddress { get; set; } = "127.0.0.1";
    public int Port { get; set; } = 12345;
    public IPEndPoint ServerIpEndPoint => new(IPAddress.Parse(IpAddress), Port);
    public int MaxMilliseconds { get; set; } = 100;
    public int ProcessorCount { get; set; } = 1;
    // true인 경우 LockTime 대기 시간에 1밀리초를 추가
    public bool AfterLockTime { get; set; } = true;
    public LingerOption LingerOption { get; set; } = new(true, 10);
}