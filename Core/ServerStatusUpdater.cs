namespace Plus.Core
{
    using System;
    using System.Timers;

    public class ServerStatusUpdater
    {
        public ServerStatusUpdater()
        {
            var timer = new Timer
            {
                Interval = 1000
            };

            timer.Elapsed += ElapsedTimer;
            timer.Enabled = true;
        }

        private static void ElapsedTimer(object sender, ElapsedEventArgs e)
        {
            if (Program.GameContext == null)
            {
                return;
            }

            var uptime = DateTime.Now - Program.ServerStarted;
            var usersOnline = Program.GameContext.PlayerController.Count;
            var roomCount = Program.GameContext.GetRoomManager().Count;

            Console.Title = $"Plus Emulator - Users Online: {usersOnline} - Rooms Loaded: {roomCount} - Uptime: {uptime.Days} day(s), {uptime.Hours} hour(s) and {uptime.Minutes} minute(s) uptime.";
        }
    }
}
