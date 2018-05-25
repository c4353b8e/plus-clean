namespace Plus.Core.Logging
{
    using System;
    using System.IO;

    public class Logger<TClass> : ILogger
    {
        private readonly Type _className;

        public Logger()
        {
            _className = typeof(TClass);
        }

        public void Trace(string message, bool log = false)
        {
            Log(message, ConsoleColor.White);
        }

        public void Warn(string message, bool log = false)
        {
            Log(message, ConsoleColor.Yellow);
        }

        public void Debug(string message, bool log = false)
        {
            Log(message, ConsoleColor.Cyan);
        }

        public void Error(string message, bool log = true)
        {
            Log(message, ConsoleColor.Red);
        }

        public void Error(string message, Exception e, bool log = true)
        {
            Log(message + Environment.NewLine + e, ConsoleColor.Red);
        }

        public void Error(Exception e)
        {
            Log("An error occurred: " + Environment.NewLine + e, ConsoleColor.Red);
        }

        public void Log(string message, ConsoleColor color, bool log = false)
        {
            var oldColor = Console.ForegroundColor;
            SetForegroundColor(color);
            Console.WriteLine($"[{GetDateTime()}] " + message);
            SetForegroundColor(oldColor);

            string logFile;

            switch (color)
            {
                case ConsoleColor.Yellow:
                    logFile = "error.log";
                    break;
                case ConsoleColor.Cyan:
                    logFile = "debug.log";
                    break;
                case ConsoleColor.Red:
                    logFile = "error.log";
                    break;
                default:
                    logFile = "trace.log";
                    break;
            }
            
            LogToFile(logFile, $"Occurred at [{DateTime.Now:MM/dd HH:mm:ss}] in [{_className.FullName}]: " + message);
        }

        private static void LogToFile(string file, string content)
        {
            var executionPath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            var fileWriter = new StreamWriter(executionPath + "/resources/logging/" + file, true);

            fileWriter.WriteLine(content);
            fileWriter.Close();
        }

        private static void SetForegroundColor(ConsoleColor color)
        {
            Console.ForegroundColor = color;
        }

        private static string GetDateTime()
        {
            return $"{DateTime.Now:MM/dd HH:mm:ss}";
        }
    }
}
