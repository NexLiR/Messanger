using ChatApp.MVVM.Model;

namespace ChatApp.Core.Interfaces
{
    public interface IServerConnection
    {
        event Action<MessageModel> OnMessageReceived;
        event Action<UserModel> OnUserConnected;
        event Action<string> OnUserDisconnected;

        Task ConnectAsync(string userName);
        Task SendMessageAsync(string message);
        Task DisconnectAsync();
        Task UseExistingConnectionAsync(string userName);
    }
}
