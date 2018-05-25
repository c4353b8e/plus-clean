namespace Plus.Core
{
    using System;
    using Communication.Packets.Outgoing.Moderation;
    using Logging;

    public class ConsoleCommands
    {
        private static readonly ILogger Logger = new Logger<ConsoleCommands>();

        public static void InvokeCommand(string inputData)
        {
            if (string.IsNullOrEmpty(inputData))
            {
                return;
            }

            try
            {
                var parameters = inputData.Split(' ');

                switch (parameters[0].ToLower())
                {
                    #region stop
                    case "stop":
                    case "shutdown":
                        {
                            Logger.Warn("The server is saving users furniture, rooms, etc. WAIT FOR THE SERVER TO CLOSE, DO NOT EXIT THE PROCESS IN TASK MANAGER!!");
                            Program.Dispose(null, null);
                            break;
                        }
                    #endregion

                    #region alert
                    case "alert":
                        {
                            var notice = inputData.Substring(6);

                            Program.GameContext.PlayerController.SendPacket(new BroadcastMessageAlertComposer(Program.LanguageManager.TryGetValue("server.console.alert") + "\n\n" + notice));

                            Logger.Trace("Alert successfully sent.");
                            break;
                        }
                    #endregion

                    default:
                        {
                            Logger.Error(parameters[0].ToLower() + " is an unknown or unsupported command. Type help for more information");
                            break;
                        }
                }
            }
            catch (Exception e)
            {
                Logger.Error("Error in command [" + inputData + "]: " + e);
            }
        }
    }
}