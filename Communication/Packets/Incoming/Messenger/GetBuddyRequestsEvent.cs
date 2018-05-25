namespace Plus.Communication.Packets.Incoming.Messenger
{
    using System.Collections.Generic;
    using System.Linq;
    using HabboHotel.GameClients;
    using HabboHotel.Users.Messenger;
    using Outgoing.Messenger;

    internal class GetBuddyRequestsEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            ICollection<MessengerRequest> requests = session.GetHabbo().GetMessenger().GetRequests().ToList();

            session.SendPacket(new BuddyRequestsComposer(requests));
        }
    }
}
