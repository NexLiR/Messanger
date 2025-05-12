namespace ChatApp.Core.Interfaces
{
    public interface IServerConnection
    {
        event Action<string> OnMessageReceived;
        event Action<string> OnUserConnected;
        event Action<string> OnUserDisconnected;

        Task ConnectAsync(string userName);
        Task SendMessageAsync(string message);
        Task DisconnectAsync();
    }
}
