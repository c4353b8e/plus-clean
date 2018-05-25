namespace Plus.Communication.Packets.Outgoing.Inventory.Trading
{
    internal class TradingConfirmedComposer : ServerPacket
    {
        public TradingConfirmedComposer(int UserId, bool Confirmed)
            : base(ServerPacketHeader.TradingConfirmedMessageComposer)
        {
            WriteInteger(UserId);
            WriteInteger(Confirmed ? 1 : 0);
        }
    }
}
