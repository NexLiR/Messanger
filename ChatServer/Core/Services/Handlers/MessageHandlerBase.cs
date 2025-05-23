using ChatServer.Core.Interfaces;

namespace ChatServer.Core.Services.Handlers
{
    public abstract class MessageHandlerBase
    {
        protected MessageHandlerBase? Next { get; private set; }

        public MessageHandlerBase SetNext(MessageHandlerBase next)
        {
            Next = next;
            return next;
        }

        public virtual async Task HandleAsync(IClient client, string message)
        {
            if (Next != null)
                await Next.HandleAsync(client, message);
        }
    }
}
