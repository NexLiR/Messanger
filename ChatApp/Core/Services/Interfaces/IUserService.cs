namespace ChatApp.Core.Services.Interfaces
{
    public interface IUserService
    {
        IEnumerable<string> GetUserNames();
        void AddUser(string userName);
        void RemoveUser(string userName);
        void ClearUsers();
    }
}