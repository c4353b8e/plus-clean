namespace Plus.Communication.Packets.Incoming.Moderation
{
    using Game.Moderation;
    using Game.Players;
    using Game.Users.Authenticator;
    using Utilities;

    internal class ModerationBanEvent : IPacketEvent
    {
        public void Parse(Player session, ClientPacket packet)
        {
            if (session == null || session.GetHabbo() == null || !session.GetHabbo().GetPermissions().HasRight("mod_soft_ban"))
            {
                return;
            }

            var userId = packet.PopInt();
            var message = packet.PopString();
            var length = packet.PopInt() * 3600 + UnixUtilities.GetNow();
            packet.PopString(); //unk1
            packet.PopString(); //unk2
            var ipBan = packet.PopBoolean();
            var machineBan = packet.PopBoolean();

            if (machineBan)
            {
                ipBan = false;
            }

            var habbo = HabboFactory.GetHabboById(userId);

            if (habbo == null)
            {
                session.SendWhisper("An error occoured whilst finding that user in the database.");
                return;
            }

            if (habbo.GetPermissions().HasRight("mod_tool") && !session.GetHabbo().GetPermissions().HasRight("mod_ban_any"))
            {
                session.SendWhisper("Oops, you cannot ban that user.");
                return;
            }

            message = message != null ? message : "No reason specified.";

            using (var dbClient = Program.DatabaseManager.GetQueryReactor())
            {
                dbClient.RunQuery("UPDATE `user_info` SET `bans` = `bans` + '1' WHERE `user_id` = '" + habbo.Id + "' LIMIT 1");
            }

            if (ipBan == false && machineBan == false)
            {
                Program.GameContext.GetModerationManager().BanUser(session.GetHabbo().Username, ModerationBanType.Username, habbo.Username, message, length);
            }
            else if (ipBan)
            {
                Program.GameContext.GetModerationManager().BanUser(session.GetHabbo().Username, ModerationBanType.IP, habbo.Username, message, length);
            }
            else
            {
                Program.GameContext.GetModerationManager().BanUser(session.GetHabbo().Username, ModerationBanType.IP, habbo.Username, message, length);
                Program.GameContext.GetModerationManager().BanUser(session.GetHabbo().Username, ModerationBanType.Username, habbo.Username, message, length);
                Program.GameContext.GetModerationManager().BanUser(session.GetHabbo().Username, ModerationBanType.Machine, habbo.Username, message, length);
            }

            var targetClient = Program.GameContext.PlayerController.GetClientByUsername(habbo.Username);
            if (targetClient != null)
            {
                targetClient.Disconnect();
            }
        }
    }
}