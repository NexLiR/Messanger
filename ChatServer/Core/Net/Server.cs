using ChatServer.Constants;
using ChatServer.Core.Interfaces;
using System.Net.Sockets;
using System.Net;

namespace ChatServer.Core.Net
{
    public class Server
    {
        private readonly IClientManager _clientManager;
        private readonly IMessageHandler _messageHandler;
        private TcpListener _listener;
        private bool _isRunning;

        public Server(
            IClientManager clientManager,
            IMessageHandler messageHandler)
        {
            _clientManager = clientManager ?? throw new ArgumentNullException(nameof(clientManager));
            _messageHandler = messageHandler ?? throw new ArgumentNullException(nameof(messageHandler));
        }

        public async Task StartAsync(CancellationToken cancellationToken = default)
        {
            if (_isRunning)
                return;

            _isRunning = true;
            _listener = new TcpListener(IPAddress.Parse(ServerConfig.DefaultIpAddress), ServerConfig.DefaultPort);

            try
            {
                _listener.Start();
                Console.WriteLine($"[{DateTime.Now}]: Server started on {ServerConfig.DefaultIpAddress}:{ServerConfig.DefaultPort}");

                while (_isRunning && !cancellationToken.IsCancellationRequested)
                {
                    var tcpClient = await _listener.AcceptTcpClientAsync(cancellationToken);

                    try
                    {
                        var client = new Client(tcpClient, _clientManager, _messageHandler);
                        _clientManager.AddClient(client);

                        await _clientManager.BroadcastConnectionAsync();

                        _ = Task.Run(() => client.ProcessAsync());
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[{DateTime.Now}]: Error accepting client: {ex.Message}");
                        tcpClient.Close();
                    }
                }
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine($"[{DateTime.Now}]: Server stopping due to cancellation request");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[{DateTime.Now}]: Server error: {ex.Message}");
            }
            finally
            {
                Stop();
            }
        }

        public void Stop()
        {
            if (!_isRunning)
                return;

            _isRunning = false;
            _listener?.Stop();
            Console.WriteLine($"[{DateTime.Now}]: Server stopped");
        }
    }
}
