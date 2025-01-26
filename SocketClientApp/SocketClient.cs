using System.Net;
using System.Net.Sockets;
using System.Text;

internal class SocketClient
{
    private const int LingerTimeSeconds = 10;
    private readonly string _clientId;
    private readonly IPEndPoint _ipEndPoint;
    private readonly CancellationTokenSource _cts;

    public SocketClient(string clientId, IPAddress ipAddress, int port, CancellationTokenSource cts)
    {
        _clientId = clientId;
        _ipEndPoint = new IPEndPoint(ipAddress, port);
        _cts = cts;
    }

    internal static SocketClient Create(string clientId, IPAddress ipAddress, int port, CancellationTokenSource cts)
    {
        return new SocketClient(clientId, ipAddress, port, cts);
    }

    internal async Task StartAsync()
    {
        CancellationToken cancellationToken = _cts.Token;
        using Socket server = new Socket(
            _ipEndPoint.AddressFamily,
            SocketType.Stream,
            ProtocolType.Tcp);
        server.LingerState = new LingerOption(true, LingerTimeSeconds);

        try
        {
            await server.ConnectAsync(_ipEndPoint, cancellationToken);

            (bool connectionResult, string errorMessage) = await ConnectAsync(server, cancellationToken);
            if (connectionResult)
            {

            }
            else
            {
                Console.WriteLine($"Connection failed and application stop. {errorMessage}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"System Error : {ex}");
        }
        finally
        {
            if (server.Connected)
            {
                server.Shutdown(SocketShutdown.Both);
            }
        }
    }

    private async Task<(bool result, string errorMessage)> ConnectAsync(Socket server, CancellationToken cancellationToken)
    {
        const string Eom = "<|EOM|>";
        const string Connect = "<|CONNECT|>";

        var message = $"{Connect}{_clientId}$${Eom}";
        var messageBytes = GetMessageBytes(message);
        await server.SendAsync(messageBytes, SocketFlags.None, cancellationToken);

        var buffer = new byte[1024];
        var receivedDataLength = await server.ReceiveAsync(buffer, SocketFlags.None, cancellationToken);
        var response = GetResponse(buffer, receivedDataLength);

        const string Success = "<|SUCCESS|>";
        const string Error = "<|ERROR|>";

        int eomPosition = response.IndexOf(Eom, 0);
        if (eomPosition == -1)
        {
            return (false, "Server cannot connect.");
        }

        bool connectionResult = response.StartsWith(Success);
        if (connectionResult)
        {
            return (connectionResult, string.Empty);
        }
        else
        {
            return (connectionResult, response[Error.Length..eomPosition]);
        }
    }

    private static string GetResponse(byte[] buffer, int receivedDataLength)
    {
        return Encoding.UTF8.GetString(buffer, 0, receivedDataLength);
    }

    private static byte[] GetMessageBytes(string message)
    {
        return Encoding.UTF8.GetBytes(message);
    }
}