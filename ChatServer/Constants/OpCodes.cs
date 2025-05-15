namespace ChatServer.Constants
{
    public static class OpCodes
    {
        public const byte Connected = 1;
        public const byte Message = 2;
        public const byte Disconnect = 3;
        public const byte Login = 4;
        public const byte Register = 5;
        public const byte AuthSuccess = 6;
        public const byte AuthFailed = 7;
        public const byte MessageHistory = 8;
        public const byte Identify = 9;
        public const byte Logout = 10;
        public const byte Disconnected = 11;
    }
}