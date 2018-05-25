namespace Plus.Communication.Packets.Outgoing.Inventory.Trading
{
    internal class TradingClosedComposer : ServerPacket
    {
        public TradingClosedComposer(int UserId)
            : base(ServerPacketHeader.TradingClosedMessageComposer)
        {
            WriteInteger(UserId);
            WriteInteger(0);
        }
    }
}
