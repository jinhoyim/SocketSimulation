﻿using SocketClientApp;

if (args.Length == 0)
{
    Console.WriteLine("Please run with clientId");
    Console.WriteLine("dotnet <app> <client id>");
    return;
}

var clientId = args[0];
var serverIpAddress = "127.0.0.1";
var serverPort = 12345;
var maxMilliseconds = 2000;
var processorCount = 1;

// true인 경우 LockTime 대기 시간에 1밀리초를 추가 
var afterLockTime = false;

var cts = new CancellationTokenSource();
Console.CancelKeyPress += (_, _) =>
{
    cts.Cancel();
    Console.WriteLine("App Stopping...");
};

try
{
    var config = new ClientConfig(
        clientId,
        serverIpAddress,
        serverPort,
        maxMilliseconds,
        processorCount,
        afterLockTime);
    var client = Client.Create(config, cts);
    await client.StartAsync();
}
catch (OperationCanceledException)
{
    Console.WriteLine("Operation cancelled.");
}
catch (Exception ex)
{
    Console.WriteLine($"Application Error: {ex}");
}
finally
{
    Console.WriteLine("App Stopped.");
}