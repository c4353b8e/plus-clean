namespace Plus.Communication.Packets.Outgoing.Marketplace
{
    internal class MarketplaceMakeOfferResultComposer : ServerPacket
    {
        public MarketplaceMakeOfferResultComposer(int Success)
            : base(ServerPacketHeader.MarketplaceMakeOfferResultMessageComposer)
        {
            WriteInteger(Success);
        }
    }
}
