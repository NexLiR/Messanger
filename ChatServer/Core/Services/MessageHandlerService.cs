using ChatServer.Core.Interfaces;
using ChatServer.Data.Repositories;

namespace ChatServer.Core.Services
{
    public class MessageHandlerService : IMessageHandler
    {
        private readonly IMessageRepository _messageRepository;
        private readonly IUserRepository _userRepository;
        private readonly IClientManager _clientManager;

        public MessageHandlerService(
            IClientManager clientManager,
            IMessageRepository messageRepository,
            IUserRepository userRepository)
        {
            _clientManager = clientManager ?? throw new ArgumentNullException(nameof(clientManager));
            _messageRepository = messageRepository ?? throw new ArgumentNullException(nameof(messageRepository));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        }

        public async Task HandleMessageAsync(IClient client, string message)
        {
            if (client == null)
                throw new ArgumentNullException(nameof(client));

            if (string.IsNullOrEmpty(message))
                throw new ArgumentException("Message cannot be null or empty", nameof(message));

            var user = await _userRepository.GetByUidAsync(client.UID);
            if (user == null)
            {
                Console.WriteLine($"User with UID {client.UID} not found in database.");
                return;
            }

            await _messageRepository.SaveMessageAsync(message, user.Id);

            var formattedMessage = $"[{DateTime.Now}]: [{client.UserName}]: {message}";
            await _clientManager.BroadcastMessageAsync(formattedMessage);
        }
    }
}