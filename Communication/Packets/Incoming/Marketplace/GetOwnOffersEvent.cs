namespace Plus.Communication.Packets.Incoming.Marketplace
{
    using Game.Players;
    using Outgoing.Marketplace;

    internal class GetOwnOffersEvent : IPacketEvent
    {
        public void Parse(Player session, ClientPacket packet)
        {
            session.SendPacket(new MarketPlaceOwnOffersComposer(session.GetHabbo().Id));
        }
    }
}
