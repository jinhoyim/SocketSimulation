using System.Net;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SocketServerApp;
using SocketServerApp.Communication;
using SocketServerApp.Factories;
using SocketServerApp.Output;
using SocketServerApp.Processing;
using SocketServerApp.Store;

using var cts = new CancellationTokenSource();

var builder = Host.CreateApplicationBuilder(args);
builder.Configuration.AddCommandLine(args);
builder.Services.AddOptions<ServerConfig>().Bind(builder.Configuration)
    .Validate(options =>
        IPAddress.TryParse(options.IpAddress, out _), "IP Address is invalid")
    .PostConfigure(options => options.IpAddress = "127.0.0.1");
builder.Services.AddSingleton<ISocketServer, Server>();
builder.Services.AddSingleton(cts);
builder.Services.AddSingleton<AllCilentsCommunicator>();
builder.Services.AddSingleton<ServerTerminator>();
builder.Services.AddSingleton<OutputWriter>();
builder.Services.AddSingleton<DataStore>();
builder.Services.AddSingleton<IDataStore>(provider =>
    new SaveLoggingDataStore(
        provider.GetRequiredService<DataStore>(),
        provider.GetRequiredService<OutputWriter>()));
builder.Services.AddSingleton<StartStateStore>();
builder.Services.AddSingleton<ServerListener>();
builder.Services.AddScoped<ClientHandler>();
builder.Services.AddTransient<ClientIdentifierFactory>();
builder.Services.AddTransient<ClientCommunicatorFactory>();
builder.Services.AddTransient<ServerWorkerFactory>();

var host = builder.Build();

Console.CancelKeyPress += (_, e) =>
{
    e.Cancel = true;
    cts.Cancel();
    Console.WriteLine("App Stopping...");
};

try
{
    var server = host.Services.GetRequiredService<ISocketServer>();
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
