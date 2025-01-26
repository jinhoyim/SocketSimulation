using System.Net.Sockets;
using System.Text;
using static SocketCommunicationLib.ProtocolConstants;

namespace SocketClientApp;

public class Connector(Socket server, string clientId)
{
    private const string ErrorMessageServerCannotConnect = "Server cannot connect.";
    
    public async Task<(bool connected, string errorMessage)> ConnectAsync(CancellationToken cancellationToken)
    {
        await RequestConnectAsync(cancellationToken);
        var received = await ReceiveResultAsync(cancellationToken);

        var eomPosition = received.IndexOf(Eom, 0, StringComparison.Ordinal);
        if (eomPosition == -1)
        {
            
            return (false, ErrorMessageServerCannotConnect);
        }

        var connected = received.StartsWith(Success);
        return connected ?
            (connected, string.Empty) :
            (connected, received[Error.Length..eomPosition]);
    }

    private async Task<string> ReceiveResultAsync(CancellationToken cancellationToken)
    {
        var buffer = new byte[1024];
        var receivedDataLength = await server.ReceiveAsync(buffer, SocketFlags.None, cancellationToken);
        var response = GetResponse(buffer, receivedDataLength);
        return response;
    }

    private async Task RequestConnectAsync(CancellationToken cancellationToken)
    {
        var message = $"{Connect}{clientId}{Eom}";
        var messageBytes = GetMessageBytes(message);
        await server.SendAsync(messageBytes, SocketFlags.None, cancellationToken);
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