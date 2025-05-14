using ChatServer.Data.Entities;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

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

        public async Task<User> CreateUserAsync(string username, string password)
        {
            if (await GetByUsernameAsync(username) != null)
            {
                throw new InvalidOperationException($"Username '{username}' is already taken.");
            }

            var passwordHash = HashPassword(password);
            var user = new User
            {
                UserName = username,
                UID = Guid.NewGuid(),
                Created = DateTime.Now,
                PasswordHash = passwordHash
            };

            await _context.Users.AddAsync(user);
            await SaveChangesAsync();
            return user;
        }

        public async Task<User> AuthenticateUserAsync(string username, string password)
        {
            var user = await GetByUsernameAsync(username);
            if (user == null || !VerifyPassword(password, user.PasswordHash))
            {
                return null;
            }
            return user;
        }

        private string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = Encoding.UTF8.GetBytes(password);
                byte[] hash = sha256.ComputeHash(bytes);
                return Convert.ToBase64String(hash);
            }
        }

        private bool VerifyPassword(string password, string passwordHash)
        {
            string hashedInput = HashPassword(password);
            return hashedInput == passwordHash;
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
