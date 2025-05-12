using ChatApp.Core.Services.Interfaces;
using System.Collections.ObjectModel;

namespace ChatApp.Core.Services
{
    public class MessageService : IMessageService
    {
        private readonly ObservableCollection<string> _messages = new();

        public IEnumerable<string> GetMessages() => _messages;

        public void AddMessage(string message)
        {
            if (!string.IsNullOrWhiteSpace(message))
            {
                _messages.Add(message);
            }
        }

        public void ClearMessages() => _messages.Clear();
    }
}
