namespace Plus.Communication.Packets.Outgoing.Inventory.Purse
{
    internal class CreditBalanceComposer : ServerPacket
    {
        public CreditBalanceComposer(int creditsBalance)
            : base(ServerPacketHeader.CreditBalanceMessageComposer)
        {
           WriteString(creditsBalance + ".0");
        }
    }
}
