namespace ChatApp.Core.Net.Handlers
{
    public class PacketHandlerFactory
    {
        private readonly Dictionary<byte, IPacketHandler> _handlers = new();

        public void RegisterHandler(IPacketHandler handler)
        {
            _handlers[handler.OpCode] = handler;
        }

        public IPacketHandler GetHandler(byte opCode)
        {
            if (_handlers.TryGetValue(opCode, out var handler))
            {
                return handler;
            }

            return null;
        }
    }
}
