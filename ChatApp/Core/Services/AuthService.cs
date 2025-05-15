using ChatApp.Constants;
using ChatApp.Core.Interfaces;
using ChatApp.Core.Net.IO;
using ChatApp.Core.Services.Interfaces;
using System.Net.Sockets;

namespace ChatApp.Core.Services
{
    public class AuthService : IAuthService, IDisposable
    {
        private TcpClient _client;
        private IPacketReader _packetReader;
        private NetworkStream _networkStream;
        private CancellationTokenSource _cancellationTokenSource;
        private bool _isAuthenticated = false;
        private string _currentUsername = string.Empty;
        private string _currentUserId = string.Empty;

        public event Action<string> OnAuthSuccessful;
        public event Action<string> OnAuthFailed;

        public bool IsAuthenticated => _isAuthenticated;
        public string CurrentUsername => _currentUsername;
        public string CurrentUserId => _currentUserId;

        public AuthService()
        {
            _cancellationTokenSource = new CancellationTokenSource();
        }

        private async Task EnsureConnectedAsync()
        {
            if (_client != null && _client.Connected)
            {
                _client.Close();
            }

            _client = new TcpClient();

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
                while (!_cancellationTokenSource.Token.IsCancellationRequested && _client.Connected)
                {
                    var opCode = _packetReader.ReadByte();
                    switch (opCode)
                    {
                        case OpCodes.AuthSuccess:
                            var username = _packetReader.ReadMessage();
                            var userId = _packetReader.ReadMessage();
                            _isAuthenticated = true;
                            _currentUsername = username;
                            _currentUserId = userId;
                            OnAuthSuccessful?.Invoke(username);

                            return;

                        case OpCodes.AuthFailed:
                            var errorMessage = _packetReader.ReadMessage();
                            OnAuthFailed?.Invoke(errorMessage);
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                OnAuthFailed?.Invoke($"Connection error: {ex.Message}");
            }
        }
        public TcpClient GetExistingConnection()
        {
            return _client;
        }
        public async Task RegisterAsync(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Username and password cannot be empty");

            await EnsureConnectedAsync();

            try
            {
                using var registerPacket = new PacketBuilder();
                registerPacket.WriteOpCode(OpCodes.Register);
                registerPacket.WriteMessage(username);
                registerPacket.WriteMessage(password);
                await _client.Client.SendAsync(registerPacket.GetPacketBytes(), SocketFlags.None);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Registration failed: {ex.Message}", ex);
            }
        }

        public async Task LoginAsync(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Username and password cannot be empty");

            await EnsureConnectedAsync();

            try
            {
                using var loginPacket = new PacketBuilder();
                loginPacket.WriteOpCode(OpCodes.Login);
                loginPacket.WriteMessage(username);
                loginPacket.WriteMessage(password);
                await _client.Client.SendAsync(loginPacket.GetPacketBytes(), SocketFlags.None);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Login failed: {ex.Message}", ex);
            }
        }

        public async Task LogoutAsync()
        {
            _isAuthenticated = false;
            _currentUsername = string.Empty;
            _currentUserId = string.Empty;

            if (_client != null && _client.Connected)
            {
                try
                {
                    using var logoutPacket = new PacketBuilder();
                    logoutPacket.WriteOpCode(OpCodes.Logout);
                    logoutPacket.WriteMessage(_currentUsername);
                    await _client.Client.SendAsync(logoutPacket.GetPacketBytes(), SocketFlags.None);
                }
                catch
                {
                }
                finally
                {
                    _client.Close();
                    _client = null;
                }
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
