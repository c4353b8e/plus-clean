namespace Plus.Communication.Packets.Incoming.Moderation
{
    using Game.Players;

    internal class ModerationCautionEvent : IPacketEvent
    {
        public void Parse(Player session, ClientPacket packet)
        {
            if (session == null || session.GetHabbo() == null || !session.GetHabbo().GetPermissions().HasRight("mod_caution"))
            {
                return;
            }

            var userId = packet.PopInt();
            var message = packet.PopString();

            var client = Program.GameContext.PlayerController.GetClientByUserId(userId);
            if (client == null || client.GetHabbo() == null)
            {
                return;
            }

            using (var dbClient = Program.DatabaseManager.GetQueryReactor())
            {
                dbClient.RunQuery("UPDATE `user_info` SET `cautions` = `cautions` + '1' WHERE `user_id` = '" + client.GetHabbo().Id + "' LIMIT 1");
            }

            client.SendNotification(message);
        }
    }
}