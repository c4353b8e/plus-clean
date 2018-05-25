namespace Plus.Communication.Packets.Incoming.Messenger
{
    using Game.Players;

    internal class SendMsgEvent : IPacketEvent
    {
        public void Parse(Player session, ClientPacket packet)
        {
            if (session == null || session.GetHabbo() == null || session.GetHabbo().GetMessenger() == null)
            {
                return;
            }

            var userId = packet.PopInt();
            if (userId == 0 || userId == session.GetHabbo().Id)
            {
                return;
            }

            var message = Program.GameContext.GetChatManager().GetFilter().CheckMessage(packet.PopString());
            if (string.IsNullOrWhiteSpace(message))
            {
                return;
            }


            if (session.GetHabbo().TimeMuted > 0)
            {
                session.SendNotification("Oops, you're currently muted - you cannot send messages.");
                return;
            }

            session.GetHabbo().GetMessenger().SendInstantMessage(userId, message);

        }
    }
}