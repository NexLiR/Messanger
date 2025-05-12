namespace ChatServer.Constants
{
    public static class OpCodes
    {
        public const byte Connect = 0;
        public const byte Message = 5;

        public const byte Connected = 1;
        public const byte Disconnected = 10;
    }
}