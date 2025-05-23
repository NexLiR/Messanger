using ChatServer.Core.Interfaces;
using ChatServer.Data.Repositories.Interfaces;
using ChatServer.Core.Services.Handlers;

namespace ChatServer.Core.Services
{
    public class MessageHandlerService : IMessageHandler
    {
        private readonly MessageHandlerBase _handlerChain;

        public MessageHandlerService(
            IClientManager clientManager,
            IMessageRepository messageRepository,
            IUserRepository userRepository)
        {
            var validation = new ValidationHandler();
            var userLookup = new UserLookupHandler(userRepository);
            var persistence = new MessagePersistenceHandler(messageRepository, userRepository);
            var broadcast = new BroadcastHandler(clientManager);
            var logging = new LoggingHandler();

            validation.SetNext(userLookup)
                      .SetNext(persistence)
                      .SetNext(broadcast)
                      .SetNext(logging);

            _handlerChain = validation;
        }

        public async Task HandleMessageAsync(IClient client, string message)
        {
            try
            {
                await _handlerChain.HandleAsync(client, message);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error handling message: {ex.Message}");
                throw;
            }
        }
    }
}