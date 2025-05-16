using ChatServer.Core.Interfaces;
using System.Net.Sockets;
using ChatServer.Constants;
using ChatServer.Data.Entities;
using ChatServer.Data.Repositories.Interfaces;
using ChatServer.Core.Factories.Interfaces;
using ChatServer.Core.Factories;
using ChatServer.Core.Net.ClientOperations.Interfaces;
using ChatServer.Core.Net.ClientOperations;

namespace ChatServer.Core.Net
{
    public class Client : IClient, IDisposable
    {
        private readonly IClientOperationFactory _clientOperationFactory;
        private readonly IPacketReaderFactory _packetReaderFactory;
        private readonly IClientManager _clientManager;
        private readonly IPacketBuilderFactory _packetBuilderFactory;
        private readonly IPacketReader _packetReader;
        private User _user;

        private bool _isConnected = true;
        private bool _isAuthenticated = false;

        public string UserName { get; set; }
        public Guid UID { get; set; } = Guid.Empty;
        public TcpClient ClientSocket { get; }
        public bool IsAuthenticated => _isAuthenticated;

        public Client(
            TcpClient clientSocket,
            IClientManager clientManager,
            IMessageHandler messageHandler,
            IUserRepository userRepository,
            IMessageRepository messageRepository,
            IPacketReaderFactory packetReaderFactory)
        {
            ClientSocket = clientSocket ?? throw new ArgumentNullException(nameof(clientSocket));
            _clientManager = clientManager ?? throw new ArgumentNullException(nameof(clientManager));
            _packetReaderFactory = packetReaderFactory ?? throw new ArgumentNullException(nameof(packetReaderFactory));

            var packetBuilderFactory = new PacketBuilderFactory();
            _packetBuilderFactory = packetBuilderFactory;

            var messageOperation = new MessageOperation(messageHandler);
            var registerOperation = new RegisterOperation(userRepository, packetBuilderFactory);
            var loginOperation = new LoginOperation(userRepository, packetBuilderFactory);
            var identifyOperation = new IdentifyOperation(userRepository, messageRepository, clientManager);
            var disconnectOperation = new DisconnectOperation();
            var logoutOperation = new LogoutOperation();

            _clientOperationFactory = new ClientOperationFactory(
                messageOperation,
                registerOperation,
                loginOperation,
                identifyOperation,
                disconnectOperation,
                logoutOperation);

            _packetReader = _packetReaderFactory.CreatePacketReader(ClientSocket.GetStream());

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

                        try
                        {
                            var operation = _clientOperationFactory.GetOperation(opCode);
                            await operation.ExecuteAsync(this, _packetReader);
                        }
                        catch (ArgumentException)
                        {
                            Console.WriteLine($"[{DateTime.Now}]: Unknown opcode: {opCode}");
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

        public void SendAuthFailedMessage(string message)
        {
            try
            {
                var packet = _packetBuilderFactory.CreatePacketBuilder();
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

        public async Task SendMessageHistoryAsync(IMessageRepository messageRepository)
        {
            try
            {
                var recentMessages = await messageRepository.GetRecentBroadcastMessagesAsync(30);
                var orderedMessages = recentMessages.Reverse();

                foreach (var message in orderedMessages)
                {
                    var formattedMessage = $"[{message.SentAt:yyyy-MM-dd HH:mm:ss}]: [{message.Sender.UserName}]: {message.Content}";

                    var historyPacket = _packetBuilderFactory.CreatePacketBuilder();
                    historyPacket.WriteOpCode(OpCodes.MessageHistory);
                    historyPacket.WriteMessage(formattedMessage);

                    await ClientSocket.Client.SendAsync(historyPacket.GetPacketBytes(), SocketFlags.None);
                    await Task.Delay(10);
                }

                var completionPacket = _packetBuilderFactory.CreatePacketBuilder();
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

        public void SetUser(User user)
        {
            _user = user;
        }

        public void SetAuthenticated(bool authenticated)
        {
            _isAuthenticated = authenticated;
        }

        public void Disconnect() => _isConnected = false;

        public void Dispose()
        {
            ClientSocket?.Close();
            (_packetReader as IDisposable)?.Dispose();
        }
    }
}
