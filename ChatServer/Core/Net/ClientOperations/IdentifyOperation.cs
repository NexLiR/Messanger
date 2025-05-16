using ChatServer.Core.Interfaces;
using ChatServer.Core.Net.ClientOperations.Interfaces;
using ChatServer.Data.Repositories.Interfaces;

namespace ChatServer.Core.Net.ClientOperations
{
    public class IdentifyOperation : IClientOperation
    {
        private readonly IUserRepository _userRepository;
        private readonly IMessageRepository _messageRepository;
        private readonly IClientManager _clientManager;

        public IdentifyOperation(
            IUserRepository userRepository,
            IMessageRepository messageRepository,
            IClientManager clientManager)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _messageRepository = messageRepository ?? throw new ArgumentNullException(nameof(messageRepository));
            _clientManager = clientManager ?? throw new ArgumentNullException(nameof(clientManager));
        }

        public async Task ExecuteAsync(Client client, IPacketReader packetReader)
        {
            var username = packetReader.ReadMessage();

            try
            {
                var user = await _userRepository.GetByUsernameAsync(username);
                if (user == null)
                {
                    client.SendAuthFailedMessage($"User '{username}' not found.");
                    return;
                }

                client.SetUser(user);
                client.UserName = user.UserName;
                client.UID = user.UID;
                client.SetAuthenticated(true);

                Console.WriteLine($"[{DateTime.Now}]: User {username} identified with UID {client.UID}");

                await client.SendMessageHistoryAsync(_messageRepository);

                _clientManager.AddClient(client);
                await _clientManager.BroadcastConnectionAsync();
            }
            catch (Exception ex)
            {
                client.SendAuthFailedMessage($"Identification failed: {ex.Message}");
            }
        }
    }
}
