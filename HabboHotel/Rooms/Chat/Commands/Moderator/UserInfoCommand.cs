namespace Plus.HabboHotel.Rooms.Chat.Commands.Moderator
{
    using System;
    using System.Data;
    using System.Text;
    using GameClients;

    internal class UserInfoCommand : IChatCommand
    {
        public string PermissionRequired => "command_user_info";

        public string Parameters => "%username%";

        public string Description => "View another users profile information.";

        public void Execute(GameClient Session, Room Room, string[] Params)
        {
            if (Params.Length == 1)
            {
                Session.SendWhisper("Please enter the username of the user you wish to view.");
                return;
            }

            DataRow UserData = null;
            DataRow UserInfo = null;
            var Username = Params[1];

            using (var dbClient = Program.DatabaseManager.GetQueryReactor())
            {
                dbClient.SetQuery("SELECT `id`,`username`,`mail`,`rank`,`motto`,`credits`,`activity_points`,`vip_points`,`gotw_points`,`online`,`rank_vip` FROM users WHERE `username` = @Username LIMIT 1");
                dbClient.AddParameter("Username", Username);
                UserData = dbClient.GetRow();
            }

            if (UserData == null)
            {
                Session.SendNotification("Oops, there is no user in the database with that username (" + Username + ")!");
                return;
            }

            using (var dbClient = Program.DatabaseManager.GetQueryReactor())
            {
                dbClient.SetQuery("SELECT * FROM `user_info` WHERE `user_id` = '" + Convert.ToInt32(UserData["id"]) + "' LIMIT 1");
                UserInfo = dbClient.GetRow();
                if (UserInfo == null)
                {
                    dbClient.RunQuery("INSERT INTO `user_info` (`user_id`) VALUES ('" + Convert.ToInt32(UserData["id"]) + "')");

                    dbClient.SetQuery("SELECT * FROM `user_info` WHERE `user_id` = '" + Convert.ToInt32(UserData["id"]) + "' LIMIT 1");
                    UserInfo = dbClient.GetRow();
                }
            }

            var TargetClient = Program.GameContext.GetClientManager().GetClientByUsername(Username);

            var origin = new DateTime(1970, 1, 1, 0, 0, 0, 0).AddSeconds(Convert.ToDouble(UserInfo["trading_locked"]));

            var HabboInfo = new StringBuilder();
            HabboInfo.Append(Convert.ToString(UserData["username"]) + "'s account:\r\r");
            HabboInfo.Append("Generic Info:\r");
            HabboInfo.Append("ID: " + Convert.ToInt32(UserData["id"]) + "\r");
            HabboInfo.Append("Rank: " + Convert.ToInt32(UserData["rank"]) + "\r");
            HabboInfo.Append("VIP Rank: " + Convert.ToInt32(UserData["rank_vip"]) + "\r");
            HabboInfo.Append("Email: " + Convert.ToString(UserData["mail"]) + "\r");
            HabboInfo.Append("Online Status: " + (TargetClient != null ? "True" : "False") + "\r\r");

            HabboInfo.Append("Currency Info:\r");
            HabboInfo.Append("Credits: " + Convert.ToInt32(UserData["credits"]) + "\r");
            HabboInfo.Append("Duckets: " + Convert.ToInt32(UserData["activity_points"]) + "\r");
            HabboInfo.Append("Diamonds: " + Convert.ToInt32(UserData["vip_points"]) + "\r");
            HabboInfo.Append("GOTW Points: " + Convert.ToInt32(UserData["gotw_points"]) + "\r\r");

            HabboInfo.Append("Moderation Info:\r");
            HabboInfo.Append("Bans: " + Convert.ToInt32(UserInfo["bans"]) + "\r");
            HabboInfo.Append("CFHs Sent: " + Convert.ToInt32(UserInfo["cfhs"]) + "\r");
            HabboInfo.Append("Abusive CFHs: " + Convert.ToInt32(UserInfo["cfhs_abusive"]) + "\r");
            HabboInfo.Append("Trading Locked: " + (Convert.ToInt32(UserInfo["trading_locked"]) == 0 ? "No outstanding lock" : "Expiry: " + origin.ToString("dd/MM/yyyy") + "") + "\r");
            HabboInfo.Append("Amount of trading locks: " + Convert.ToInt32(UserInfo["trading_locks_count"]) + "\r\r");

            if (TargetClient != null)
            {
                HabboInfo.Append("Current Session:\r");
                if (!TargetClient.GetHabbo().InRoom)
                {
                    HabboInfo.Append("Currently not in a room.\r");
                }
                else
                {
                    HabboInfo.Append("Room: " + TargetClient.GetHabbo().CurrentRoom.Name + " (" + TargetClient.GetHabbo().CurrentRoom.RoomId + ")\r");
                    HabboInfo.Append("Room Owner: " + TargetClient.GetHabbo().CurrentRoom.OwnerName + "\r");
                    HabboInfo.Append("Current Visitors: " + TargetClient.GetHabbo().CurrentRoom.UserCount + "/" + TargetClient.GetHabbo().CurrentRoom.UsersMax);
                }
            }
            Session.SendNotification(HabboInfo.ToString());
        }
    }
}
