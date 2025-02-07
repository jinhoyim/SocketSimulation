using System.Text;
using SocketClientApp.Communication;
using SocketCommunicationLib;
using SocketCommunicationLib.Contract;

namespace SocketClientApp;

public class SocketListenerFactory
{
    public SocketListener Create(ClientCommunicator communicator)
    {
        return new SocketListener(
            communicator.Socket,
            new SocketMessageStringExtractor(
                ProtocolConstants.Eom,Encoding.UTF8)
        );
    }
}