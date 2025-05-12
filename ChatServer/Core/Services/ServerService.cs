using ChatServer.Constants;
using ChatServer.Core.Net;

namespace ChatServer.Core.Services
{
    public class ServerService
    {
        private readonly Server _server;
        private readonly CancellationTokenSource _cts = new CancellationTokenSource();

        public ServerService(
            ClientManager clientManager,
            MessageHandlerService messageHandler)
        {
            _server = new Server(clientManager, messageHandler);
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