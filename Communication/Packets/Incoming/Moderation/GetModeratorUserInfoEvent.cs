namespace Plus.Communication.Packets.Incoming.Moderation
{
    using System.Data;
    using Outgoing.Moderation;

    internal class GetModeratorUserInfoEvent : IPacketEvent
    {
        public void Parse(HabboHotel.GameClients.GameClient session, ClientPacket packet)
        {
            if (!session.GetHabbo().GetPermissions().HasRight("mod_tool"))
            {
                return;
            }

            var userId = packet.PopInt();

            DataRow user;
            DataRow info;
            using (var dbClient = Program.DatabaseManager.GetQueryReactor())
            {
                dbClient.SetQuery("SELECT `id`,`username`,`online`,`mail`,`ip_last`,`look`,`account_created`,`last_online` FROM `users` WHERE `id` = '" + userId + "' LIMIT 1");
                user = dbClient.GetRow();

                if (user == null)
                {
                    session.SendNotification(Program.LanguageManager.TryGetValue("user.not_found"));
                    return;
                }

                dbClient.SetQuery("SELECT `cfhs`,`cfhs_abusive`,`cautions`,`bans`,`trading_locked`,`trading_locks_count` FROM `user_info` WHERE `user_id` = '" + userId + "' LIMIT 1");
                info = dbClient.GetRow();
                if (info == null)
                {
                    dbClient.RunQuery("INSERT INTO `user_info` (`user_id`) VALUES ('" + userId + "')");
                    dbClient.SetQuery("SELECT `cfhs`,`cfhs_abusive`,`cautions`,`bans`,`trading_locked`,`trading_locks_count` FROM `user_info` WHERE `user_id` = '" + userId + "' LIMIT 1");
                    info = dbClient.GetRow();
                }
            }


            session.SendPacket(new ModeratorUserInfoComposer(user, info));
        }
    }
}