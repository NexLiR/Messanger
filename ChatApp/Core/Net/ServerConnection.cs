using System.Net.Sockets;
using ChatApp.Constants;
using ChatApp.Core.Interfaces;
using ChatApp.Core.Net.IO;
using ChatApp.Core.Services.Interfaces;

namespace ChatApp.Core.Net
{
    public class ServerConnection : IServerConnection, IDisposable
    {
        private readonly IAuthService _authService;
        private TcpClient _client;
        private IPacketReader _packetReader;
        private NetworkStream _networkStream;
        private CancellationTokenSource _cancellationTokenSource;

        public event Action<string> OnMessageReceived;
        public event Action<string> OnUserConnected;
        public event Action<string> OnUserDisconnected;

        public ServerConnection(IAuthService authService)
        {
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
            _client = new TcpClient();
            _cancellationTokenSource = new CancellationTokenSource();
        }

        public async Task ConnectAsync(string userName)
        {
            if (!_authService.IsAuthenticated)
                throw new InvalidOperationException("You must be authenticated to connect");

            if (_client.Connected)
                return;

            try
            {
                await _client.ConnectAsync(ServerConfig.DefaultIpAddress, ServerConfig.DefaultPort);
                _networkStream = _client.GetStream();
                _packetReader = new PacketReader(_networkStream);

                _ = Task.Run(ProcessIncomingMessagesAsync);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Connection failed: {ex.Message}", ex);
            }
        }

        private async Task ProcessIncomingMessagesAsync()
        {
            try
            {
                while (!_cancellationTokenSource.Token.IsCancellationRequested)
                {
                    var opCode = _packetReader.ReadByte();
                    switch (opCode)
                    {
                        case OpCodes.Connected:
                            var userName = _packetReader.ReadMessage();
                            var userId = _packetReader.ReadMessage();
                            OnUserConnected?.Invoke(userName);
                            break;
                        case OpCodes.Message:
                            var message = _packetReader.ReadMessage();
                            OnMessageReceived?.Invoke(message);
                            break;
                        case OpCodes.Disconnect:
                            var disconnectedUserId = _packetReader.ReadMessage();
                            OnUserDisconnected?.Invoke(disconnectedUserId);
                            break;
                    }
                }
            }
            catch (InvalidOperationException)
            {
                OnUserDisconnected?.Invoke(null);
            }
        }

        public async Task SendMessageAsync(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
                return;

            if (!_client.Connected || !_authService.IsAuthenticated)
                throw new InvalidOperationException("Not connected or not authenticated");

            try
            {
                using var messagePacket = new PacketBuilder();
                messagePacket.WriteOpCode(OpCodes.Message);
                messagePacket.WriteMessage(message);
                await _client.Client.SendAsync(messagePacket.GetPacketBytes(), SocketFlags.None);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to send message: {ex.Message}", ex);
            }
        }

        public async Task DisconnectAsync()
        {
            _cancellationTokenSource.Cancel();

            if (_client.Connected)
            {
                _client.Close();
            }
        }

        public void Dispose()
        {
            _cancellationTokenSource.Cancel();
            _client?.Dispose();
            (_packetReader as IDisposable)?.Dispose();
        }
    }
}
