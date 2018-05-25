namespace Plus.Communication.Packets.Outgoing.Marketplace
{
    internal class MarketplaceCanMakeOfferResultComposer : ServerPacket
    {
        public MarketplaceCanMakeOfferResultComposer(int Result)
            : base(ServerPacketHeader.MarketplaceCanMakeOfferResultMessageComposer)
        {
            WriteInteger(Result);
            WriteInteger(0);
        }
    }
}
