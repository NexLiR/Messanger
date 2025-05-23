using ChatServer.Core.Interfaces;

namespace ChatServer.Core.Services.Handlers
{
    public class BroadcastHandler : MessageHandlerBase
    {
        private readonly IClientManager _clientManager;

        public BroadcastHandler(IClientManager clientManager)
        {
            _clientManager = clientManager;
        }

        public override async Task HandleAsync(IClient client, string message)
        {
            var formattedMessage = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}]: [{client.UserName}]: {message}";
            await _clientManager.BroadcastMessageAsync(formattedMessage);
            await base.HandleAsync(client, message);
        }
    }
}
