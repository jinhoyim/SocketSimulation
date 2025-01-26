using System.Net;

string clientId = "A1";
string serverIpAddress = "127.0.0.1";
int serverPort = 12345;

using var cts = new CancellationTokenSource();
Console.CancelKeyPress += (_, _) =>
{
    cts.Cancel();
    Console.WriteLine("App Stopping...");
};

if (!IPAddress.TryParse(serverIpAddress, out var ipAddress))
{
    Console.WriteLine("Invalid Server IP Address.");
    return;
}

var client = SocketClient.Create(clientId, ipAddress, serverPort, cts);
await client.StartAsync();

Console.WriteLine("App Stopped.");