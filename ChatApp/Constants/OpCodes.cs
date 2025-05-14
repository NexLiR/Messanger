namespace ChatApp.Constants
{
    public static class OpCodes
    {
        public const byte Connect = 0;
        public const byte Message = 5;
        public const byte Connected = 1;
        public const byte Disconnect = 10;

        public const byte Register = 2;
        public const byte Login = 3;
        public const byte AuthSuccess = 4;
        public const byte AuthFailed = 11;

        public const byte MessageHistory = 12;
    }
}