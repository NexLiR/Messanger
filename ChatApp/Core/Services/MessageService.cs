using ChatApp.Core.Services.Interfaces;
using ChatApp.MVVM.Model;
using System.Collections.ObjectModel;

namespace ChatApp.Core.Services
{
    public class MessageService : IMessageService
    {
        private readonly ObservableCollection<MessageModel> _messages = new();

        public IEnumerable<MessageModel> GetMessages() => _messages;

        public void AddMessage(MessageModel message)
        {
            if (message != null && !string.IsNullOrWhiteSpace(message.Content))
            {
                _messages.Add(message);
            }
        }

        public void ClearMessages() => _messages.Clear();
    }
}
