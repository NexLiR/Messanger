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
        private bool _isConnected = false;

        public event Action<string> OnMessageReceived;
        public event Action<string> OnUserConnected;
        public event Action<string> OnUserDisconnected;

        public ServerConnection(IAuthService authService)
        {
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
            _cancellationTokenSource = new CancellationTokenSource();
        }

        public async Task ConnectAsync(string userName)
        {
            if (!_authService.IsAuthenticated)
                throw new InvalidOperationException("You must be authenticated to connect");

            if (_isConnected)
                return;

            try
            {
                _client = new TcpClient();
                await _client.ConnectAsync(ServerConfig.DefaultIpAddress, ServerConfig.DefaultPort);
                _networkStream = _client.GetStream();
                _packetReader = new PacketReader(_networkStream);
                _isConnected = true;

                using (var identifyPacket = new PacketBuilder())
                {
                    identifyPacket.WriteOpCode(OpCodes.Identify);
                    identifyPacket.WriteMessage(_authService.CurrentUsername);
                    await _client.Client.SendAsync(identifyPacket.GetPacketBytes(), SocketFlags.None);
                }

                _ = Task.Run(ProcessIncomingMessagesAsync);
            }
            catch (Exception ex)
            {
                _isConnected = false;
                throw new InvalidOperationException($"Connection failed: {ex.Message}", ex);
            }
        }

        private async Task ProcessIncomingMessagesAsync()
        {
            try
            {
                while (!_cancellationTokenSource.Token.IsCancellationRequested && _client.Connected)
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
                        case OpCodes.MessageHistory:
                            var historyMessage = _packetReader.ReadMessage();
                            OnMessageReceived?.Invoke(historyMessage);
                            break;
                        case OpCodes.Disconnect:
                            var disconnectedUserId = _packetReader.ReadMessage();
                            OnUserDisconnected?.Invoke(disconnectedUserId);
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                OnUserDisconnected?.Invoke(null);
                _isConnected = false;
            }
        }

        public async Task SendMessageAsync(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
                return;

            if (!_client.Connected || !_authService.IsAuthenticated || !_isConnected)
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
            if (!_isConnected)
                return;

            _isConnected = false;
            _cancellationTokenSource.Cancel();

            try
            {
                if (_client.Connected)
                {
                    using var disconnectPacket = new PacketBuilder();
                    disconnectPacket.WriteOpCode(OpCodes.Disconnect);
                    disconnectPacket.WriteMessage(_authService.CurrentUsername);
                    await _client.Client.SendAsync(disconnectPacket.GetPacketBytes(), SocketFlags.None);

                    _client.Close();
                }
            }
            catch (Exception)
            {
            }
        }

        public async Task UseExistingConnectionAsync(string userName)
        {
            if (!_authService.IsAuthenticated)
                throw new InvalidOperationException("You must be authenticated to connect");

            if (_isConnected)
                return;

            try
            {
                _client = _authService.GetExistingConnection();

                if (_client == null || !_client.Connected)
                    throw new InvalidOperationException("No active connection available");

                _networkStream = _client.GetStream();
                _packetReader = new PacketReader(_networkStream);
                _isConnected = true;

                using (var identifyPacket = new PacketBuilder())
                {
                    identifyPacket.WriteOpCode(OpCodes.Identify);
                    identifyPacket.WriteMessage(_authService.CurrentUsername);
                    await _client.Client.SendAsync(identifyPacket.GetPacketBytes(), SocketFlags.None);
                }

                _ = Task.Run(ProcessIncomingMessagesAsync);
            }
            catch (Exception ex)
            {
                _isConnected = false;
                throw new InvalidOperationException($"Connection failed: {ex.Message}", ex);
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
