using System.Net.Sockets;
using ChatApp.Constants;
using ChatApp.Core.Interfaces;
using ChatApp.Core.Net.Handlers;
using ChatApp.Core.Net.Handlers.PacketHandlers;
using ChatApp.Core.Net.IO;
using ChatApp.Core.Services.Interfaces;
using ChatApp.MVVM.Model;

namespace ChatApp.Core.Net
{
    public class ServerConnection : IServerConnection, IDisposable
    {
        private readonly IAuthService _authService;
        private readonly PacketHandlerFactory _handlerFactory;
        private readonly IMessageService _messageService;
        private readonly IUserService _userService;
        private TcpClient _client;
        private IPacketReader _packetReader;
        private NetworkStream _networkStream;
        private CancellationTokenSource _cancellationTokenSource;
        private bool _isConnected = false;

        public event Action<MessageModel> OnMessageReceived;
        public event Action<UserModel> OnUserConnected;
        public event Action<string> OnUserDisconnected;

        public ServerConnection(IAuthService authService, IMessageService messageService, IUserService userService)
        {
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
            _messageService = messageService ?? throw new ArgumentNullException(nameof(messageService));
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _cancellationTokenSource = new CancellationTokenSource();
            _handlerFactory = new PacketHandlerFactory();

            RegisterHandlers();
        }

        private void RegisterHandlers()
        {
            RegisterConnectedHandler();
            RegisterMessageHandler();
            RegisterHistoryHandler();
            RegisterDisconnectHandlers();
        }

        private void RegisterConnectedHandler()
        {
            var connectedHandler = new ConnectedPacketHandler(_userService);
            connectedHandler.OnUserConnected += userName => {
                var userModel = new UserModel { UserName = userName };
                _userService.AddUser(userModel);
                OnUserConnected?.Invoke(userModel);
            };
            _handlerFactory.RegisterHandler(connectedHandler);
        }

        private void RegisterMessageHandler()
        {
            var messageHandler = new MessagePacketHandler();
            messageHandler.OnMessageReceived += messageStr => {
                var messageModel = CreateMessageModel(messageStr, "System");
                _messageService.AddMessage(messageModel);
                OnMessageReceived?.Invoke(messageModel);
            };
            _handlerFactory.RegisterHandler(messageHandler);
        }

        private void RegisterHistoryHandler()
        {
            var historyHandler = new MessageHistoryPacketHandler();
            historyHandler.OnMessageReceived += messageStr => {
                var messageModel = CreateMessageModel(messageStr, "History");
                _messageService.AddMessage(messageModel);
                OnMessageReceived?.Invoke(messageModel);
            };
            _handlerFactory.RegisterHandler(historyHandler);
        }

        private void RegisterDisconnectHandlers()
        {
            var disconnectHandler = new DisconnectPacketHandler();
            disconnectHandler.OnUserDisconnected += userId => {
                _userService.RemoveUser(userId);
                OnUserDisconnected?.Invoke(userId);
            };
            _handlerFactory.RegisterHandler(disconnectHandler);

            var disconnectedHandler = new DisconnectedPacketHandler();
            disconnectedHandler.OnUserDisconnected += userId => {
                _userService.RemoveUser(userId);
                OnUserDisconnected?.Invoke(userId);
            };
            _handlerFactory.RegisterHandler(disconnectedHandler);
        }

        private MessageModel CreateMessageModel(string messageStr, string defaultSender)
        {
            try
            {
                var timeAndRest = messageStr.Split("]: ", 2);
                var timePart = timeAndRest[0].TrimStart('[');
                var rest = timeAndRest[1];

                var userAndMessage = rest.Split("]: ", 2);
                var user = userAndMessage[0].TrimStart('[');
                var message = userAndMessage[1];

                return new MessageModel
                {
                    Timestamp = DateTime.Parse(timePart),
                    SenderName = user,
                    Content = message
                };
            }
            catch
            {
                return new MessageModel
                {
                    SenderName = defaultSender,
                    Content = messageStr,
                    Timestamp = DateTime.Now
                };
            }
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
                await InitializeConnectionAsync();
            }
            catch (Exception ex)
            {
                _isConnected = false;
                throw new InvalidOperationException($"Connection failed: {ex.Message}", ex);
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

                await InitializeConnectionAsync();
            }
            catch (Exception ex)
            {
                _isConnected = false;
                throw new InvalidOperationException($"Connection failed: {ex.Message}", ex);
            }
        }

        private async Task InitializeConnectionAsync()
        {
            _networkStream = _client.GetStream();
            _packetReader = new PacketReader(_networkStream);
            _isConnected = true;

            await SendIdentificationAsync();
            _ = Task.Run(ProcessIncomingMessagesAsync);
        }

        private async Task SendIdentificationAsync()
        {
            using var identifyPacket = new PacketBuilder();
            identifyPacket.WriteOpCode(OpCodes.Identify);
            identifyPacket.WriteMessage(_authService.CurrentUsername);
            await _client.Client.SendAsync(identifyPacket.GetPacketBytes(), SocketFlags.None);
        }

        private async Task ProcessIncomingMessagesAsync()
        {
            try
            {
                while (!_cancellationTokenSource.Token.IsCancellationRequested && _client.Connected)
                {
                    var opCode = _packetReader.ReadByte();
                    var handler = _handlerFactory.GetHandler(opCode);

                    if (handler != null)
                    {
                        await handler.HandleAsync(_packetReader);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Connection error: {ex.Message}");
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
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to disconnect: {ex.Message}", ex);
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
