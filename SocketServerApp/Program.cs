using SocketServerApp;

var hostIpAddress = "127.0.0.1";
var hostPort = 12345;
var startConnectionCount = 5;
var endCount = 10;
var initLockTimeSeconds = 2;
var socketConnectionQueue = 1000;
var serverTerminatedDelaySeconds = 5;

using var cts = new CancellationTokenSource();
Console.CancelKeyPress += (_, e) =>
{
    e.Cancel = true;
    cts.Cancel();
    Console.WriteLine("App Stopping...");
};

try
{
    var config = new ServerConfig(
        hostIpAddress,
        hostPort,
        startConnectionCount,
        endCount,
        initLockTimeSeconds,
        socketConnectionQueue,
        serverTerminatedDelaySeconds);
    
    var server = Server.Create(config, cts);
    await server.StartAsync(cts.Token);
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
finally
{
    Console.WriteLine("App Stopped.");
}