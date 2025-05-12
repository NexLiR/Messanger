using ChatServer.Core.Interfaces;
using ChatServer.Core.Net;
using ChatServer.Data.Repositories;

namespace ChatServer.Core.Services
{
    public class ServerService
    {
        private readonly IClientManager _clientManager;
        private readonly IMessageHandler _messageHandler;
        private readonly IUserRepository _userRepository;
        private Server _server;
        private readonly CancellationTokenSource _cts = new CancellationTokenSource();

        public ServerService(
            IClientManager clientManager,
            IMessageHandler messageHandler,
            IUserRepository userRepository)
        {
            _clientManager = clientManager ?? throw new ArgumentNullException(nameof(clientManager));
            _messageHandler = messageHandler ?? throw new ArgumentNullException(nameof(messageHandler));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));

            _server = new Server(_clientManager, _messageHandler, _userRepository);
        }

        public async Task StartAsync()
        {
            try
            {
                await _server.StartAsync(_cts.Token);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Server failed to start: {ex.Message}");
            }
        }

        public void Stop()
        {
            _cts.Cancel();
            _server.Stop();
        }
    }
}