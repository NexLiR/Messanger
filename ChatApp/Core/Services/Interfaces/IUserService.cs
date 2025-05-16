using ChatApp.MVVM.Model;

namespace ChatApp.Core.Services.Interfaces
{
    public interface IUserService
    {
        IEnumerable<UserModel> GetUsers();
        void AddUser(UserModel user);
        void RemoveUser(string userId);
        void ClearUsers();
    }
}