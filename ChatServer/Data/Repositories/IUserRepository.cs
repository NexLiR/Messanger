using ChatServer.Data.Entities;

namespace ChatServer.Data.Repositories
{
    public interface IUserRepository
    {
        Task<User> GetByUidAsync(Guid uid);
        Task<User> GetByUsernameAsync(string username);
        Task<User> CreateUserAsync(string username, Guid uid);
        Task<IEnumerable<User>> GetAllUsersAsync();
        Task SaveChangesAsync();
    }
}