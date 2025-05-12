namespace ChatServer.Core.Interfaces
{
    public interface IMessageHandler
    {
        Task HandleMessageAsync(IClient client, string message);
    }
}