using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Text;

namespace SocketServerApp;

public class ConnectionAcceptor(Socket client, ConcurrentDictionary<string, Socket> clients)
{
    private const string Eom = "<|EOM|>";
    private const string Connect = "<|CONNECT|>";
    private const string Success = "<|SUCCESS|>";
    private const string Error = "<|ERROR|>";
    
    public async Task<(bool accepted, string clientId)> AcceptAsync(CancellationToken cancellationToken)
    {
        var receiveRequest = await ReceiveRequestAsync(cancellationToken);
        string clientId = ExtractClientId(receiveRequest);
        var accepted = await SendAcceptedAsync(clientId, cancellationToken);
        return (accepted, clientId);
    }

    private async Task<string> ReceiveRequestAsync(CancellationToken cancellationToken)
    {
        var buffer = new byte[1024];
        var receivedBytes = await client.ReceiveAsync(buffer, cancellationToken);
        var receiveRequest = Encoding.UTF8.GetString(buffer, 0, receivedBytes);    
        return receiveRequest;
    }
    
    private static string ExtractClientId(string receiveRequest)
    {
        if (!receiveRequest.StartsWith(Connect)) return string.Empty;
        
        var eomPosition = receiveRequest.IndexOf(Eom, 0, StringComparison.Ordinal);
        return receiveRequest[Connect.Length..eomPosition];
    }
    
    private async Task<bool> SendAcceptedAsync(string clientId, CancellationToken cancellationToken)
    {
        string responseMessage;
        var accepted = false;
        
        if (string.IsNullOrEmpty(clientId))
        {
            responseMessage = $"{Error}Required connect with ClientId.{Eom}";
        }
        else if (clients.ContainsKey(clientId))
        {
            responseMessage = $"{Error}ClientId already connected.{Eom}";
        }
        else
        {
            accepted = true;
            responseMessage = $"{Success}Connection successful.{Eom}";
        }
        
        var messageBytes = GetMessageBytes(responseMessage);
        await client.SendAsync(messageBytes, SocketFlags.None, cancellationToken);
        return accepted;
    }
    
    private static byte[] GetMessageBytes(string ackMessage)
    {
        return Encoding.UTF8.GetBytes(ackMessage);
    }
}