using ChatServer.Core.Interfaces;
using System.Net.Sockets;
using ChatServer.Constants;
using ChatServer.Core.Net.IO;
using ChatServer.Data.Entities;
using ChatServer.Data.Repositories;

namespace ChatServer.Core.Net
{
    public class Client : IClient, IDisposable
    {
        private readonly IUserRepository _userRepository;
        private User _user;

        private readonly IClientManager _clientManager;
        private readonly IMessageHandler _messageHandler;
        private readonly IPacketReader _packetReader;
        private bool _isConnected = true;

        public string UserName { get; set; }
        public Guid UID { get; }
        public TcpClient ClientSocket { get; }

        public Client(
             TcpClient clientSocket,
             IClientManager clientManager,
             IMessageHandler messageHandler,
             IUserRepository userRepository)
        {
            ClientSocket = clientSocket ?? throw new ArgumentNullException(nameof(clientSocket));
            _clientManager = clientManager ?? throw new ArgumentNullException(nameof(clientManager));
            _messageHandler = messageHandler ?? throw new ArgumentNullException(nameof(messageHandler));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));

            UID = Guid.NewGuid();
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

        private async Task HandleRegisterAsync(string username, string password)
        {
            try
            {
                var user = await _userRepository.CreateUserAsync(username, password);

                UserName = user.UserName;
                _user = user;

                using var packet = new PacketBuilder();
                packet.WriteOpCode(OpCodes.AuthSuccess);
                packet.WriteMessage(user.UserName);
                packet.WriteMessage(user.UID.ToString());

                await ClientSocket.Client.SendAsync(packet.GetPacketBytes(), SocketFlags.None);

                Console.WriteLine($"[{DateTime.Now}]: User {username} registered successfully");
                await _clientManager.BroadcastConnectionAsync();
            }
            catch (Exception ex)
            {
                using var packet = new PacketBuilder();
                packet.WriteOpCode(OpCodes.AuthFailed);
                packet.WriteMessage($"Registration failed: {ex.Message}");

                await ClientSocket.Client.SendAsync(packet.GetPacketBytes(), SocketFlags.None);
                Console.WriteLine($"[{DateTime.Now}]: Registration failed for {username}: {ex.Message}");
            }
        }

        private async Task HandleLoginAsync(string username, string password)
        {
            try
            {
                var user = await _userRepository.AuthenticateUserAsync(username, password);

                if (user == null)
                {
                    using var failedPacket = new PacketBuilder();
                    failedPacket.WriteOpCode(OpCodes.AuthFailed);
                    failedPacket.WriteMessage("Invalid username or password");

                    await ClientSocket.Client.SendAsync(failedPacket.GetPacketBytes(), SocketFlags.None);
                    Console.WriteLine($"[{DateTime.Now}]: Login failed for {username}: Invalid credentials");
                    return;
                }

                UserName = user.UserName;
                _user = user;

                using var packet = new PacketBuilder();
                packet.WriteOpCode(OpCodes.AuthSuccess);
                packet.WriteMessage(user.UserName);
                packet.WriteMessage(user.UID.ToString());

                await ClientSocket.Client.SendAsync(packet.GetPacketBytes(), SocketFlags.None);

                Console.WriteLine($"[{DateTime.Now}]: User {username} logged in successfully");
                await _clientManager.BroadcastConnectionAsync();
            }
            catch (Exception ex)
            {
                using var packet = new PacketBuilder();
                packet.WriteOpCode(OpCodes.AuthFailed);
                packet.WriteMessage($"Login failed: {ex.Message}");

                await ClientSocket.Client.SendAsync(packet.GetPacketBytes(), SocketFlags.None);
                Console.WriteLine($"[{DateTime.Now}]: Login failed for {username}: {ex.Message}");
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
