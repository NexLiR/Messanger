namespace ChatApp.Core.Services.Interfaces
{
    public interface IMessageService
    {
        IEnumerable<string> GetMessages();
        void AddMessage(string message);
        void ClearMessages();
    }
}
