namespace Plus.Communication.Packets.Incoming.Users
{
    using System;
    using System.Linq;
    using HabboHotel.GameClients;
    using HabboHotel.Users.Authenticator;
    using Outgoing.Users;

    internal class GetRelationshipsEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            var habbo = HabboFactory.GetHabboById(packet.PopInt());
            if (habbo == null)
            {
                return;
            }

            var rand = new Random();
            habbo.Relationships = habbo.Relationships.OrderBy(x => rand.Next()).ToDictionary(item => item.Key, item => item.Value);

            session.SendPacket(new GetRelationshipsComposer(habbo));
        }
    }
}
