using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace SocketServerApp
{
    internal class SocketServer
    {
        private const int LingerTimeSeconds = 10;
        private readonly IPEndPoint _ipEndPoint;
        private readonly CancellationTokenSource _cts;

        private bool _isStarted = false;
        private readonly Lock _lock = new Lock();
        private readonly ConcurrentDictionary<string, Socket> clients = new();

        public SocketServer(IPAddress ipAddress, int port, CancellationTokenSource cts)
        {
            _ipEndPoint = new IPEndPoint(ipAddress, port);
            _cts = cts;
        }

        internal static SocketServer Create(IPAddress ipAddress, int port, CancellationTokenSource cts)
        {
            return new SocketServer(ipAddress, port, cts);
        }

        internal async Task StartAsync()
        {
            CancellationToken cancellationToken = _cts.Token;
            using Socket listener = new Socket(
                        _ipEndPoint.AddressFamily,
                        SocketType.Stream,
                        ProtocolType.Tcp);
            listener.LingerState = new LingerOption(true, LingerTimeSeconds);
            listener.Bind(_ipEndPoint);
            listener.Listen(1000);

            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    Socket client = await listener.AcceptAsync(cancellationToken);
                    client.LingerState = new LingerOption(true, LingerTimeSeconds);

                    _ = Task.Run(async () =>
                    {

                        try
                        {
                            StringBuilder sb = new StringBuilder();

                            var buffer = new byte[1024];

                            int receivedBytes = await client.ReceiveAsync(buffer, cancellationToken);
                            var request = Encoding.UTF8.GetString(buffer, 0, receivedBytes);
                            const string Eom = "<|EOM|>";
                            const string Connect = "<|CONNECT|>";
                            const string Success = "<|SUCCESS|>";
                            const string Error = "<|ERROR|>";

                            string responseMessage;
                            bool connectionResult = false;

                            if (request.StartsWith(Connect))
                            {
                                int eomPosition = request.IndexOf(Eom, 0);
                                string clientId = request[Connect.Length..eomPosition];

                                if (string.IsNullOrEmpty(clientId))
                                {
                                    responseMessage = $"{Error}Invalid ClientId.{Eom}";
                                }
                                else if (clients.ContainsKey(clientId))
                                {
                                    responseMessage = $"{Error}ClientId already connected.{Eom}";
                                }
                                else
                                {
                                    connectionResult = true;
                                    clients.AddOrUpdate(clientId, client, (_, _) => client);
                                    responseMessage = $"{Success}Connection successful.{Eom}";
                                }
                            }
                            else
                            {
                                responseMessage = $"{Error}Required connect with ClientId.{Eom}";
                            }

                            var messageBytes = GetMessageBytes(responseMessage);
                            await client.SendAsync(messageBytes, SocketFlags.None, cancellationToken);

                            if (!connectionResult)
                            {
                                return;
                            }
                        }
                        catch (SocketException se)
                        {
                            Console.WriteLine($"Socket Error : {se.SocketErrorCode}({se.ErrorCode}) {se.Message}");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"System Error : {ex}");
                        }
                        finally
                        {
                            if (client.Connected)
                            {
                                client.Shutdown(SocketShutdown.Both);
                            }
                            client.Dispose();

                            if (clients.Count == 0)
                            {
                                _cts.Cancel();
                            }
                        }
                        Console.WriteLine("Client Socket closed.");
                    }, cancellationToken);
                }
            }
            catch (OperationCanceledException oce)
            {
#if DEBUG
                Console.WriteLine($"Operation Canceled : {oce.Message}");
#endif
            }
            catch (Exception ex)
            {
                Console.WriteLine($"System Error : {ex}");
            }
            finally
            {
                if (listener.Connected)
                {
                    listener.Shutdown(SocketShutdown.Both);
                }
            }
            Console.WriteLine("Listener socket closed.");
        }

        private static byte[] GetMessageBytes(string ackMessage)
        {
            return Encoding.UTF8.GetBytes(ackMessage);
        }
    }
}
