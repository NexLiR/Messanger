using ChatServer.Data.Entities;

namespace ChatServer.Data.Repositories
{
    public interface IMessageRepository
    {
        Task<Message> SaveMessageAsync(string content, int senderId, int? recipientId = null);
        Task<IEnumerable<Message>> GetRecentBroadcastMessagesAsync(int count = 50);
        Task SaveChangesAsync();
    }
}
