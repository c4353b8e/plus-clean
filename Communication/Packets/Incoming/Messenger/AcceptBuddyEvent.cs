namespace Plus.Communication.Packets.Incoming.Messenger
{
    using HabboHotel.GameClients;

    internal class AcceptBuddyEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            if (session?.GetHabbo() == null || session.GetHabbo().GetMessenger() == null)
            {
                return;
            }

            var amount = packet.PopInt();
            if (amount > 50)
            {
                amount = 50;
            }
            else if (amount < 0)
            {
                return;
            }

            for (var i = 0; i < amount; i++)
            {
                var requestId = packet.PopInt();

                if (!session.GetHabbo().GetMessenger().TryGetRequest(requestId, out var request))
                {
                    continue;
                }

                if (request.To != session.GetHabbo().Id)
                {
                    return;
                }

                if (!session.GetHabbo().GetMessenger().FriendshipExists(request.To))
                {
                    session.GetHabbo().GetMessenger().CreateFriendship(request.From);
                }

                session.GetHabbo().GetMessenger().HandleRequest(requestId);
            }
        }
    }
}
