using ChatServer.Core.Net;

namespace ChatServer.Core.Services
{
    public class ServerService
    {
        private readonly Server _server;
        private readonly CancellationTokenSource _cts = new CancellationTokenSource();

        public ServerService(Server server)
        {
            _server = server ?? throw new ArgumentNullException(nameof(server));
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