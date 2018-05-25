namespace Plus.Communication.Packets.Incoming.Inventory.Badges
{
    using Game.Players;
    using Outgoing.Inventory.Badges;

    internal class GetBadgesEvent : IPacketEvent
    {
        public void Parse(Player session, ClientPacket packet)
        {
            session.SendPacket(new BadgesComposer(session));
        }
    }
}
