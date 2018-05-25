namespace Plus.Communication.Packets.Incoming.Messenger
{
    internal class DeclineBuddyEvent : IPacketEvent
    {
        public void Parse(HabboHotel.GameClients.GameClient session, ClientPacket packet)
        {
            if (session == null || session.GetHabbo() == null || session.GetHabbo().GetMessenger() == null)
            {
                return;
            }

            var declineAll = packet.PopBoolean();
            packet.PopInt(); //amount
         
            if (!declineAll)
            {
                var requestId = packet.PopInt();
                session.GetHabbo().GetMessenger().HandleRequest(requestId);
            }
            else
            {
                session.GetHabbo().GetMessenger().HandleAllRequests();
            }
        }
    }
}
