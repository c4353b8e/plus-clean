namespace Plus.HabboHotel.Rooms.Chat.Commands.User
{
    using System;
    using System.Text;

    internal class StatsCommand : IChatCommand
    {
        public string PermissionRequired => "command_stats";

        public string Parameters => "";

        public string Description => "View your current statistics.";

        public void Execute(GameClients.GameClient Session, Room Room, string[] Params)
        {
            var Minutes = Session.GetHabbo().GetStats().OnlineTime / 60;
            var Hours = Minutes / 60;
            var OnlineTime = Convert.ToInt32(Hours);
            var s = OnlineTime == 1 ? "" : "s";

            var HabboInfo = new StringBuilder();
            HabboInfo.Append("Your account stats:\r\r");

            HabboInfo.Append("Currency Info:\r");
            HabboInfo.Append("Credits: " + Session.GetHabbo().Credits + "\r");
            HabboInfo.Append("Duckets: " + Session.GetHabbo().Duckets + "\r");
            HabboInfo.Append("Diamonds: " + Session.GetHabbo().Diamonds + "\r");
            HabboInfo.Append("Online Time: " + OnlineTime + " Hour" + s + "\r");
            HabboInfo.Append("Respects: " + Session.GetHabbo().GetStats().Respect + "\r");
            HabboInfo.Append("GOTW Points: " + Session.GetHabbo().GOTWPoints + "\r\r");


            Session.SendNotification(HabboInfo.ToString());
        }
    }
}
