namespace Plus.Communication.Packets.Outgoing.Inventory.Trading
{
    internal class TradingErrorComposer : ServerPacket
    {
        public TradingErrorComposer(int Error, string Username)
            : base(ServerPacketHeader.TradingErrorMessageComposer)
        {
            WriteInteger(Error);
           WriteString(Username);
        }
    }
}
