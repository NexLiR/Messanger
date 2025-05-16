using ChatServer.Core.Factories.Interfaces;
using ChatServer.Core.Interfaces;
using ChatServer.Core.Net;
using ChatServer.Data.Repositories.Interfaces;
using System.Net.Sockets;

namespace ChatServer.Core.Factories
{
    public class ClientFactory : IClientFactory
    {
        private readonly IClientManager _clientManager;
        private readonly IMessageHandler _messageHandler;
        private readonly IUserRepository _userRepository;
        private readonly IMessageRepository _messageRepository;
        private readonly IPacketReaderFactory _packetReaderFactory;

        public ClientFactory(
            IClientManager clientManager,
            IMessageHandler messageHandler,
            IUserRepository userRepository,
            IMessageRepository messageRepository,
            IPacketReaderFactory packetReaderFactory)
        {
            _clientManager = clientManager ?? throw new ArgumentNullException(nameof(clientManager));
            _messageHandler = messageHandler ?? throw new ArgumentNullException(nameof(messageHandler));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _messageRepository = messageRepository ?? throw new ArgumentNullException(nameof(messageRepository));
            _packetReaderFactory = packetReaderFactory ?? throw new ArgumentNullException(nameof(packetReaderFactory));
        }

        public IClient CreateClient(TcpClient clientSocket)
        {
            if (clientSocket == null)
                throw new ArgumentNullException(nameof(clientSocket));

            return new Client(
                clientSocket,
                _clientManager,
                _messageHandler,
                _userRepository,
                _messageRepository,
                _packetReaderFactory);
        }
    }
}
