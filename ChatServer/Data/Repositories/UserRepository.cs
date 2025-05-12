using ChatServer.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace ChatServer.Data.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly ChatDbContext _context;

        public UserRepository(ChatDbContext context)
        {
            _context = context;
        }

        public async Task<User> GetByUidAsync(Guid uid)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.UID == uid);
        }

        public async Task<User> GetByUsernameAsync(string username)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.UserName == username);
        }

        public async Task<User> CreateUserAsync(string username, Guid uid)
        {
            var existingUser = await GetByUsernameAsync(username);
            if (existingUser != null)
            {
                await SaveChangesAsync();
                return existingUser;
            }

            var user = new User
            {
                UserName = username,
                UID = uid,
                Created = DateTime.Now,
            };

            await _context.Users.AddAsync(user);
            await SaveChangesAsync();
            return user;
        }

        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            return await _context.Users.ToListAsync();
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
