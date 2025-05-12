using ChatApp.Core.Services.Interfaces;
using System.Collections.ObjectModel;

namespace ChatApp.Core.Services
{
    public class UserService : IUserService
    {
        private readonly ObservableCollection<string> _users = new();

        public IEnumerable<string> GetUserNames() => _users;

        public void AddUser(string userName)
        {
            if (!string.IsNullOrWhiteSpace(userName) && !_users.Contains(userName))
            {
                _users.Add(userName);
            }
        }

        public void RemoveUser(string userName)
        {
            if (!string.IsNullOrWhiteSpace(userName))
            {
                _users.Remove(userName);
            }
        }

        public void ClearUsers() => _users.Clear();
    }
}
