namespace Plus.Communication.Packets.Incoming.Marketplace
{
    using HabboHotel.GameClients;
    using Outgoing.Marketplace;

    internal class GetMarketplaceCanMakeOfferEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            var errorCode = session.GetHabbo().TradingLockExpiry > 0 ? 6 : 1;

            session.SendPacket(new MarketplaceCanMakeOfferResultComposer(errorCode));
        }
    }
}