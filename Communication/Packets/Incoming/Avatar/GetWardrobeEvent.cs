namespace Plus.Communication.Packets.Incoming.Avatar
{
    using Game.Players;
    using Outgoing.Avatar;

    internal class GetWardrobeEvent : IPacketEvent
    {
        public void Parse(Player session, ClientPacket packet)
        {
            session.SendPacket(new WardrobeComposer(session.GetHabbo().Id));
        }
    }
}
