using System.Net;
using SocketClientApp;

var clientId = "A1";
var serverIpAddress = "127.0.0.1";
var serverPort = 12345;

var cts = new CancellationTokenSource();
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
try
{
    await client.StartAsync();
    Console.WriteLine("App Stopped.");
}
catch (Exception ex)
{
    Console.WriteLine($"Application Error: {ex}");
}
