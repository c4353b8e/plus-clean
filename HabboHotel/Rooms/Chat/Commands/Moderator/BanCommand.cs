namespace Plus.HabboHotel.Rooms.Chat.Commands.Moderator
{
    using System;
    using GameClients;
    using Moderation;
    using Users.Authenticator;
    using Utilities;

    internal class BanCommand : IChatCommand
    {

        public string PermissionRequired => "command_ban";

        public string Parameters => "%username% %length% %reason% ";

        public string Description => "Remove a toxic player from the hotel for a fixed amount of time.";

        public void Execute(GameClient Session, Room Room, string[] Params)
        {
            if (Params.Length == 1)
            {
                Session.SendWhisper("Please enter the username of the user you'd like to IP ban & account ban.");
                return;
            }

            var Habbo = HabboFactory.GetHabboByUsername(Params[1]);
            if (Habbo == null)
            {
                Session.SendWhisper("An error occoured whilst finding that user in the database.");
                return;
            }

            if (Habbo.GetPermissions().HasRight("mod_soft_ban") && !Session.GetHabbo().GetPermissions().HasRight("mod_ban_any"))
            {
                Session.SendWhisper("Oops, you cannot ban that user.");
                return;
            }

            double Expire = 0;
            var Hours = Params[2];
            if (string.IsNullOrEmpty(Hours) || Hours == "perm")
            {
                Expire = UnixTimestamp.GetNow() + 78892200;
            }
            else
            {
                Expire = UnixTimestamp.GetNow() + Convert.ToDouble(Hours) * 3600;
            }

            string Reason = null;
            if (Params.Length >= 4)
            {
                Reason = CommandManager.MergeParams(Params, 3);
            }
            else
            {
                Reason = "No reason specified.";
            }

            var Username = Habbo.Username;
            using (var dbClient = Program.DatabaseManager.GetQueryReactor())
            {
                dbClient.RunQuery("UPDATE `user_info` SET `bans` = `bans` + '1' WHERE `user_id` = '" + Habbo.Id + "' LIMIT 1");
            }

            Program.GameContext.GetModerationManager().BanUser(Session.GetHabbo().Username, ModerationBanType.Username, Habbo.Username, Reason, Expire);

            var TargetClient = Program.GameContext.GetClientManager().GetClientByUsername(Username);
            if (TargetClient != null)
            {
                TargetClient.Disconnect();
            }

            Session.SendWhisper("Success, you have account banned the user '" + Username + "' for " + Hours + " hour(s) with the reason '" + Reason + "'!");
        }
    }
}