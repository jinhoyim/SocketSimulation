﻿using SocketServerApp;
using System.Net;

var serverIpAddress = "127.0.0.1";
var serverPort = 12345;

using var cts = new CancellationTokenSource();
Console.CancelKeyPress += (_, e) =>
{
    e.Cancel = true;
    cts.Cancel();
    Console.WriteLine("App Stopping...");
};

if (!IPAddress.TryParse(serverIpAddress, out var ipAddress))
{
    Console.WriteLine("Invalid Server IP Address.");
    return;
}

var server = SocketServer.Create(ipAddress, serverPort, cts);
try
{
    await server.StartAsync(cts.Token);
    Console.WriteLine("App Stopped.");
}
catch (OperationCanceledException oce)
{
#if DEBUG
    Console.WriteLine($"Operation Canceled : {oce.Message}");
#endif
}
catch (Exception ex)
{
    Console.WriteLine($"Application Error: {ex}");
}