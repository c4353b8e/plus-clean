namespace Plus.Communication.Packets.Incoming.Moderation
{
    using Game.Players;

    internal class ModerationMsgEvent : IPacketEvent
    {
        public void Parse(Player session, ClientPacket packet)
        {
            if (session == null || session.GetHabbo() == null || !session.GetHabbo().GetPermissions().HasRight("mod_alert"))
            {
                return;
            }

            var userId = packet.PopInt();
            var message = packet.PopString();

            var client = Program.GameContext.PlayerController.GetClientByUserId(userId);
            if (client == null)
            {
                return;
            }

            client.SendNotification(message);
        }
    }
}
