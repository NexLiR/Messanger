using ChatServer.Core.Interfaces;
using System.Net.Sockets;
using ChatServer.Constants;
using ChatServer.Core.Net.IO;
using ChatServer.Data.Entities;
using ChatServer.Data.Repositories.Interfaces;

namespace ChatServer.Core.Net
{
    public class Client : IClient, IDisposable
    {
        private readonly IUserRepository _userRepository;
        private readonly IMessageRepository _messageRepository;
        private User _user;

        private readonly IClientManager _clientManager;
        private readonly IMessageHandler _messageHandler;
        private readonly IPacketReader _packetReader;
        private bool _isConnected = true;
        private bool _isAuthenticated = false;

        public string UserName { get; set; }
        public Guid UID { get; private set; } = Guid.Empty;
        public TcpClient ClientSocket { get; }

        public Client(
             TcpClient clientSocket,
             IClientManager clientManager,
             IMessageHandler messageHandler,
             IUserRepository userRepository,
             IMessageRepository messageRepository)
        {
            ClientSocket = clientSocket ?? throw new ArgumentNullException(nameof(clientSocket));
            _clientManager = clientManager ?? throw new ArgumentNullException(nameof(clientManager));
            _messageHandler = messageHandler ?? throw new ArgumentNullException(nameof(messageHandler));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _messageRepository = messageRepository ?? throw new ArgumentNullException(nameof(messageRepository));

            _packetReader = new PacketReader(ClientSocket.GetStream());

            Console.WriteLine($"[{DateTime.Now}]: New connection established: {ClientSocket.Client.RemoteEndPoint}");
        }

        public async Task ProcessAsync()
        {
            try
            {
                while (_isConnected && ClientSocket.Connected)
                {
                    try
                    {
                        var opCode = _packetReader.ReadByte();

                        switch (opCode)
                        {
                            case OpCodes.Message:
                                if (!_isAuthenticated)
                                {
                                    SendAuthFailedMessage("You must authenticate first.");
                                    continue;
                                }

                                var message = _packetReader.ReadMessage();
                                Console.WriteLine($"[{DateTime.Now}]: Message received from {UserName}: {message}");
                                await _messageHandler.HandleMessageAsync(this, message);
                                break;

                            case OpCodes.Register:
                                var registerUsername = _packetReader.ReadMessage();
                                var registerPassword = _packetReader.ReadMessage();
                                await HandleRegisterAsync(registerUsername, registerPassword);
                                break;

                            case OpCodes.Login:
                                var loginUsername = _packetReader.ReadMessage();
                                var loginPassword = _packetReader.ReadMessage();
                                await HandleLoginAsync(loginUsername, loginPassword);
                                break;

                            case OpCodes.Identify:
                                var identifyUsername = _packetReader.ReadMessage();
                                await HandleIdentifyAsync(identifyUsername);
                                break;

                            case OpCodes.Disconnect:
                                var username = _packetReader.ReadMessage();
                                Console.WriteLine($"[{DateTime.Now}]: User {username} requested disconnect");
                                _isConnected = false;
                                break;

                            case OpCodes.Logout:
                                var logoutUsername = _packetReader.ReadMessage();
                                Console.WriteLine($"[{DateTime.Now}]: User {logoutUsername} logged out");
                                _isAuthenticated = false;
                                _user = null;
                                break;

                            default:
                                Console.WriteLine($"[{DateTime.Now}]: Unknown opcode: {opCode}");
                                break;
                        }
                    }
                    catch (InvalidOperationException ex)
                    {
                        Console.WriteLine($"[{DateTime.Now}]: [{UID}]: Processing error: {ex.Message}");
                        _isConnected = false;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[{DateTime.Now}]: [{UID}]: Disconnected - Error: {ex.Message}");
            }
            finally
            {
                if (_isConnected)
                {
                    _isConnected = false;
                    await _clientManager.BroadcastDisconnectAsync(UID.ToString());
                }

                Dispose();
            }
        }

        private async Task HandleIdentifyAsync(string username)
        {
            try
            {
                var user = await _userRepository.GetByUsernameAsync(username);

                if (user == null)
                {
                    SendAuthFailedMessage($"User '{username}' not found.");
                    return;
                }

                _user = user;
                UserName = user.UserName;
                UID = user.UID;
                _isAuthenticated = true;

                Console.WriteLine($"[{DateTime.Now}]: User {username} identified with UID {UID}");

                await SendMessageHistoryAsync();

                await _clientManager.BroadcastConnectionAsync();
            }
            catch (Exception ex)
            {
                SendAuthFailedMessage($"Identification failed: {ex.Message}");
            }
        }

        private async Task HandleRegisterAsync(string username, string password)
        {
            try
            {
                var user = await _userRepository.CreateUserAsync(username, password);

                UserName = user.UserName;
                UID = user.UID;
                _user = user;
                _isAuthenticated = true;

                using var packet = new PacketBuilder();
                packet.WriteOpCode(OpCodes.AuthSuccess);
                packet.WriteMessage(user.UserName);
                packet.WriteMessage(user.UID.ToString());

                await ClientSocket.Client.SendAsync(packet.GetPacketBytes(), SocketFlags.None);

                Console.WriteLine($"[{DateTime.Now}]: User {username} registered successfully with UID {UID}");
            }
            catch (Exception ex)
            {
                SendAuthFailedMessage($"Registration failed: {ex.Message}");
            }
        }

        private async Task HandleLoginAsync(string username, string password)
        {
            try
            {
                var user = await _userRepository.AuthenticateUserAsync(username, password);

                if (user == null)
                {
                    SendAuthFailedMessage("Invalid username or password");
                    return;
                }

                UserName = user.UserName;
                UID = user.UID;
                _user = user;
                _isAuthenticated = true;

                using var packet = new PacketBuilder();
                packet.WriteOpCode(OpCodes.AuthSuccess);
                packet.WriteMessage(user.UserName);
                packet.WriteMessage(user.UID.ToString());

                await ClientSocket.Client.SendAsync(packet.GetPacketBytes(), SocketFlags.None);

                Console.WriteLine($"[{DateTime.Now}]: User {username} logged in successfully with UID {UID}");
            }
            catch (Exception ex)
            {
                SendAuthFailedMessage($"Login failed: {ex.Message}");
            }
        }

        private void SendAuthFailedMessage(string message)
        {
            try
            {
                using var packet = new PacketBuilder();
                packet.WriteOpCode(OpCodes.AuthFailed);
                packet.WriteMessage(message);

                ClientSocket.Client.Send(packet.GetPacketBytes());
                Console.WriteLine($"[{DateTime.Now}]: Auth failed: {message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[{DateTime.Now}]: Error sending auth failed message: {ex.Message}");
            }
        }

        private async Task SendMessageHistoryAsync()
        {
            try
            {
                var recentMessages = await _messageRepository.GetRecentBroadcastMessagesAsync(30);

                var orderedMessages = recentMessages.Reverse();

                foreach (var message in orderedMessages)
                {
                    var formattedMessage = $"[{message.SentAt.ToString("yyyy-MM-dd HH:mm:ss")}]: [{message.Sender.UserName}]: {message.Content}";

                    using var historyPacket = new PacketBuilder();
                    historyPacket.WriteOpCode(OpCodes.MessageHistory);
                    historyPacket.WriteMessage(formattedMessage);

                    await ClientSocket.Client.SendAsync(historyPacket.GetPacketBytes(), SocketFlags.None);

                    await Task.Delay(10);
                }

                using var completionPacket = new PacketBuilder();
                completionPacket.WriteOpCode(OpCodes.MessageHistory);
                completionPacket.WriteMessage("--- End of message history ---");
                await ClientSocket.Client.SendAsync(completionPacket.GetPacketBytes(), SocketFlags.None);

                Console.WriteLine($"[{DateTime.Now}]: Message history sent to {UserName}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[{DateTime.Now}]: Error sending message history: {ex.Message}");
            }
        }

        public void Disconnect()
        {
            _isConnected = false;
        }

        public void Dispose()
        {
            if (ClientSocket != null && ClientSocket.Connected)
            {
                ClientSocket.Close();
            }

            (_packetReader as IDisposable)?.Dispose();
        }
    }
}
