namespace Plus.Communication.Packets.Outgoing.BuildersClub
{
    internal class BCBorrowedItemsComposer : ServerPacket
    {
        public BCBorrowedItemsComposer()
            : base(ServerPacketHeader.BCBorrowedItemsMessageComposer)
        {
            WriteInteger(0);
        }
    }
}
