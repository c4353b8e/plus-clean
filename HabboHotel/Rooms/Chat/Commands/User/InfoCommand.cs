namespace Plus.HabboHotel.Rooms.Chat.Commands.User
{
    using System;
    using Communication.Packets.Outgoing.Rooms.Notifications;
    using GameClients;

    internal class InfoCommand : IChatCommand
    {
        public string PermissionRequired => "command_info";

        public string Parameters => "";

        public string Description => "Displays generic information that everybody loves to see.";

        public void Execute(GameClient Session, Room Room, string[] Params)
        {
            var Uptime = DateTime.Now - Program.ServerStarted;
            var OnlineUsers = Program.GameContext.GetClientManager().Count;
            var RoomCount = Program.GameContext.GetRoomManager().Count;

            Session.SendPacket(new RoomNotificationComposer("Powered by PlusEmulator",
                 "<b>Credits</b>:\n" +
                 "DevBest Community\n\n" +
                 "<b>Current run time information</b>:\n" +
                 "Online Users: " + OnlineUsers + "\n" +
                 "Rooms Loaded: " + RoomCount + "\n" +
                 "Uptime: " + Uptime.Days + " day(s), " + Uptime.Hours + " hours and " + Uptime.Minutes + " minutes.\n\n" +
                 "<b>SWF Revision</b>:\n" + Program.GameContext.GameRevision, "plus", ""));
        }
    }
}