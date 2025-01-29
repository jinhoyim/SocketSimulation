using System.Net;

public class ClientConfig
{
    public string ClientId { get; }
    public IPEndPoint ServerIpEndPoint { get; }
    public int MaxMilliseconds { get; }

    public ClientConfig(
        string clientId,
        string serverIpAddress,
        int serverPort,
        int maxMilliseconds)
    {
        if (!IPAddress.TryParse(serverIpAddress, out var ip))
        {
            throw new ArgumentException("Invalid IP address.", nameof(serverIpAddress));
        }

        ClientId = clientId;
        ServerIpEndPoint = new IPEndPoint(ip, serverPort);
        MaxMilliseconds = maxMilliseconds;
    }
}