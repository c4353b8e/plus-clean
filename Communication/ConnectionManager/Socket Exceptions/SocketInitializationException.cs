namespace Plus.Communication.ConnectionManager.Socket_Exceptions
{
    using System;

    public class SocketInitializationException : Exception
    {
        public SocketInitializationException(string message)
            : base(message)
        {
        }
    }
}