namespace Plus.Communication.Packets.Outgoing.Inventory.Furni
{
    internal class FurniListRemoveComposer : ServerPacket
    {
        public FurniListRemoveComposer(int Id)
            : base(ServerPacketHeader.FurniListRemoveMessageComposer)
        {
            WriteInteger(Id);
        }
    }
}
