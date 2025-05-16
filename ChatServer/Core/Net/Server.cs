using ChatServer.Constants;
using ChatServer.Core.Interfaces;
using System.Net.Sockets;
using System.Net;
using ChatServer.Core.Factories.Interfaces;

namespace ChatServer.Core.Net
{
    public class Server : ServerBase
    {
        private readonly IClientFactory _clientFactory;

        public Server(
            IClientManager clientManager,
            IClientFactory clientFactory)
            : base(clientManager)
        {
            _clientFactory = clientFactory ?? throw new ArgumentNullException(nameof(clientFactory));
        }

        protected override void InitializeListener()
        {
            _listener = new TcpListener(IPAddress.Parse(ServerConfig.DefaultIpAddress), ServerConfig.DefaultPort);
        }

        protected override async Task HandleNewClientAsync(TcpClient tcpClient)
        {
            try
            {
                var client = _clientFactory.CreateClient(tcpClient);
                _ = Task.Run(() => client.ProcessAsync());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[{DateTime.Now}]: Error accepting client: {ex.Message}");
                tcpClient.Close();
            }
        }

        protected override void OnServerStarted()
        {
            Console.WriteLine($"[{DateTime.Now}]: Server started on {ServerConfig.DefaultIpAddress}:{ServerConfig.DefaultPort}");
        }

        protected override void OnServerStopped()
        {
            Console.WriteLine($"[{DateTime.Now}]: Server stopped");
        }
    }
}
