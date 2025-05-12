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

        public string UserName { get; }
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

            try
            {
                var opcode = _packetReader.ReadByte();
                if (opcode != OpCodes.Connect)
                {
                    throw new InvalidOperationException($"Expected connect opcode {OpCodes.Connect}, got {opcode}");
                }

                UserName = _packetReader.ReadMessage();

                _user = _userRepository.CreateUserAsync(UserName, UID).GetAwaiter().GetResult();

                Console.WriteLine($"[{DateTime.Now}]: Client <{UserName}> connected: {ClientSocket.Client.RemoteEndPoint}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during client initialization: {ex.Message}");
                Dispose();
                throw;
            }
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
