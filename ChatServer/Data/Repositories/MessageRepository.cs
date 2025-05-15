using ChatServer.Data.Entities;
using ChatServer.Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ChatServer.Data.Repositories
{
    public class MessageRepository : IMessageRepository
    {
        private readonly ChatDbContext _context;

        public MessageRepository(ChatDbContext context)
        {
            _context = context;
        }

        public async Task<Message> SaveMessageAsync(string content, int senderId, int? recipientId = null)
        {
            var message = new Message
            {
                Content = content,
                SenderId = senderId,
                RecipientId = recipientId,
                SentAt = DateTime.Now,
                IsBroadcast = recipientId == null
            };

            await _context.Messages.AddAsync(message);
            await SaveChangesAsync();

            return message;
        }

        public async Task<IEnumerable<Message>> GetRecentBroadcastMessagesAsync(int count = 30)
        {
            return await _context.Messages
                .Include(m => m.Sender)
                .Where(m => m.IsBroadcast)
                .OrderByDescending(m => m.SentAt)
                .Take(count)
                .ToListAsync();
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
