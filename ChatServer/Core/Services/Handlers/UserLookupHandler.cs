using ChatServer.Core.Interfaces;
using ChatServer.Data.Repositories.Interfaces;

namespace ChatServer.Core.Services.Handlers
{
    public class UserLookupHandler : MessageHandlerBase
    {
        private readonly IUserRepository _userRepository;

        public UserLookupHandler(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public override async Task HandleAsync(IClient client, string message)
        {
            var user = await _userRepository.GetByUidAsync(client.UID);
            if (user == null)
            {
                Console.WriteLine($"User with UID {client.UID} not found in database.");
                return;
            }

            await base.HandleAsync(client, message);
        }
    }
}
