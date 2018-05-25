namespace Plus.Communication.Packets.Incoming.Moderation
{
    using HabboHotel.GameClients;

    internal class ModerationMsgEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            if (session == null || session.GetHabbo() == null || !session.GetHabbo().GetPermissions().HasRight("mod_alert"))
            {
                return;
            }

            var userId = packet.PopInt();
            var message = packet.PopString();

            var client = Program.GameContext.GetClientManager().GetClientByUserId(userId);
            if (client == null)
            {
                return;
            }

            client.SendNotification(message);
        }
    }
}
