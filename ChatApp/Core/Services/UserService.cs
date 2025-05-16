using ChatApp.Core.Services.Interfaces;
using ChatApp.MVVM.Model;
using System.Collections.ObjectModel;

namespace ChatApp.Core.Services
{
    public class UserService : IUserService
    {
        private readonly ObservableCollection<UserModel> _users = new();

        public IEnumerable<UserModel> GetUsers() => _users;

        public void AddUser(UserModel user)
        {
            if (user != null && !string.IsNullOrWhiteSpace(user.UserName) &&
                !_users.Any(u => u.UserId == user.UserId))
            {
                _users.Add(user);
            }
        }

        public void RemoveUser(string userId)
        {
            if (!string.IsNullOrWhiteSpace(userId))
            {
                var user = _users.FirstOrDefault(u => u.UserId == userId);
                if (user != null)
                {
                    _users.Remove(user);
                }
            }
        }

        public void ClearUsers() => _users.Clear();
    }
}
