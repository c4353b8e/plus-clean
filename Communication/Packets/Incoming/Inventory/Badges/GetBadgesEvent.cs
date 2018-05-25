namespace Plus.Communication.Packets.Incoming.Inventory.Badges
{
    using HabboHotel.GameClients;
    using Outgoing.Inventory.Badges;

    internal class GetBadgesEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            session.SendPacket(new BadgesComposer(session));
        }
    }
}
