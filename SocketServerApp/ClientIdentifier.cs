using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Text;
using static SocketCommunicationLib.ProtocolConstants;

namespace SocketServerApp;

public class ClientIdentifier
{
    private readonly Socket _client;
    private readonly ICollection<string> _clients;
    
    private VerifyStatus _verifyStatus = VerifyStatus.PreVerification;
    public bool IsVerified => _verifyStatus == VerifyStatus.Verified; 
    public string ClientId { get; private set; } = string.Empty;

    public ClientIdentifier(Socket client, ICollection<string> clients)
    {
        _client = client;
        _clients = clients;
    }
    
    public async Task IdentifyClientAsync(CancellationToken cancellationToken)
    {
        var receiveRequest = await ReceiveRequestAsync(cancellationToken);
        var tmpClientId = ExtractClientId(receiveRequest);
        _verifyStatus = Verify(tmpClientId);
        if (_verifyStatus == VerifyStatus.Verified)
        {
            ClientId = tmpClientId;
        }
        await SendIdentifyResultAsync(cancellationToken);
    }

    private async Task<string> ReceiveRequestAsync(CancellationToken cancellationToken)
    {
        var buffer = new byte[1024];
        var receivedBytes = await _client.ReceiveAsync(buffer, cancellationToken);
        var receiveRequest = Encoding.UTF8.GetString(buffer, 0, receivedBytes);    
        return receiveRequest;
    }
    
    private static string ExtractClientId(string receiveRequest)
    {
        if (!receiveRequest.StartsWith(Connect)) return string.Empty;
        
        var eomPosition = receiveRequest.IndexOf(Eom, 0, StringComparison.Ordinal);
        return receiveRequest[Connect.Length..eomPosition];
    }

    private VerifyStatus Verify(string clientId)
    {
        return clientId switch
        {
            null or "" => VerifyStatus.RequiredClientId,
            _ when _clients.Contains(clientId) => VerifyStatus.ClientIdAlreadyConnected,
            _ => VerifyStatus.Verified
        };
    }
    
    private async Task SendIdentifyResultAsync(CancellationToken cancellationToken)
    {
        var responseMessage = _verifyStatus switch
        {
            VerifyStatus.RequiredClientId => $"{Error}Required connect with ClientId.{Eom}",
            VerifyStatus.ClientIdAlreadyConnected => $"{Error}ClientId already connected.{Eom}",
            VerifyStatus.Verified => $"{Success}Connection successful.{Eom}",
            VerifyStatus.PreVerification => $"{Error}Verify was not performed.{Eom}",
            _ => throw new ArgumentOutOfRangeException(nameof(_verifyStatus), _verifyStatus, null)
        };
        
        var messageBytes = Encoding.UTF8.GetBytes(responseMessage);
        await _client.SendAsync(messageBytes, SocketFlags.None, cancellationToken);
    }

    private enum VerifyStatus
    {
        PreVerification,
        Verified,
        RequiredClientId,
        ClientIdAlreadyConnected
    }
}