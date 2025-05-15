using ChatServer.Data.Entities;

namespace ChatServer.Data.Repositories.Interfaces
{
    public interface IUserRepository
    {
        Task<User> GetByUidAsync(Guid uid);
        Task<User> GetByUsernameAsync(string username);
        Task<User> CreateUserAsync(string username, string password);
        Task<User> AuthenticateUserAsync(string username, string password);
        Task<IEnumerable<User>> GetAllUsersAsync();
        Task SaveChangesAsync();
    }
}