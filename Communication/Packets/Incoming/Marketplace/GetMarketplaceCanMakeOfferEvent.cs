namespace Plus.Communication.Packets.Incoming.Marketplace
{
    using Game.Players;
    using Outgoing.Marketplace;

    internal class GetMarketplaceCanMakeOfferEvent : IPacketEvent
    {
        public void Parse(Player session, ClientPacket packet)
        {
            var errorCode = session.GetHabbo().TradingLockExpiry > 0 ? 6 : 1;

            session.SendPacket(new MarketplaceCanMakeOfferResultComposer(errorCode));
        }
    }
}