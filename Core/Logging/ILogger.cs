namespace Plus.Core.Logging
{
    using System;

    public interface ILogger
    {
        void Trace(string message, bool log = false);
        void Warn(string message, bool log = false);
        void Debug(string message, bool log = false);
        void Error(string message, bool log = false);
        void Error(string message, Exception e, bool log = true);
        void Error(Exception e);
        void Log(string message, ConsoleColor color, bool log = false);
    }
}
