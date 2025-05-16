using ChatApp.MVVM.Model;

namespace ChatApp.Core.Services.Interfaces
{
    public interface IMessageService
    {
        IEnumerable<MessageModel> GetMessages();
        void AddMessage(MessageModel message);
        void ClearMessages();
    }
}
