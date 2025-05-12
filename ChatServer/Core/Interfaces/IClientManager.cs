namespace ChatServer.Core.Interfaces
{
    public interface IClientManager
    {
        IReadOnlyList<IClient> Clients { get; }
        void AddClient(IClient client);
        void RemoveClient(IClient client);
        Task BroadcastConnectionAsync();
        Task BroadcastMessageAsync(string message);
        Task BroadcastDisconnectAsync(string uid);
    }
}