using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SocketClientApp;
using SocketClientApp.Output;
using SocketClientApp.Processing;
using SocketClientApp.Store;

if (args.Length == 0)
{
    Console.WriteLine("Please run with clientId");
    Console.WriteLine("dotnet <app> --clientId=<client id>");
    return;
}

var cts = new CancellationTokenSource();

var builder = Host.CreateApplicationBuilder(args);
builder.Configuration.AddCommandLine(args);
builder.Services.AddOptions<ClientConfig>().Bind(builder.Configuration)
    .Validate(options => !string.IsNullOrWhiteSpace(options.ClientId));
builder.Services.AddSingleton<ISocketClient, Client>();
builder.Services.AddSingleton<ConnectorFactory>();
builder.Services.AddSingleton<ClientCommunicatorFactory>();
builder.Services.AddSingleton<ClientJobProcessorFactory>();
builder.Services.AddSingleton<SocketListenerFactory>();
builder.Services.AddScoped<ClientWorker>();
builder.Services.AddScoped<CountStore>();
builder.Services.AddScoped<OutputWriter>();
builder.Services.AddScoped<LockTimesStore>();
builder.Services.AddScoped<NextDataGenerator>();
builder.Services.AddSingleton(cts);

var app = builder.Build();

Console.CancelKeyPress += (_, _) =>
{
    cts.Cancel();
    Console.WriteLine("App Stopping...");
};

try
{
    var client = app.Services.GetRequiredService<ISocketClient>();
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