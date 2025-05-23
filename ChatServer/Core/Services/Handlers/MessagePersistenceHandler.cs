using ChatServer.Core.Interfaces;
using ChatServer.Data.Repositories.Interfaces;

namespace ChatServer.Core.Services.Handlers
{
    public class MessagePersistenceHandler : MessageHandlerBase
    {
        private readonly IMessageRepository _messageRepository;
        private readonly IUserRepository _userRepository;

        public MessagePersistenceHandler(IMessageRepository messageRepository, IUserRepository userRepository)
        {
            _messageRepository = messageRepository;
            _userRepository = userRepository;
        }

        public override async Task HandleAsync(IClient client, string message)
        {
            var user = await _userRepository.GetByUidAsync(client.UID);
            if (user != null)
            {
                await _messageRepository.SaveMessageAsync(message, user.Id);
            }
            await base.HandleAsync(client, message);
        }
    }
}
