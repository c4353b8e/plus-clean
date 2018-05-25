namespace Plus.Communication.Packets.Outgoing.Rooms.Avatar
{
    internal class CarryObjectComposer : ServerPacket
    {
        public CarryObjectComposer(int virtualID, int itemID)
            : base(ServerPacketHeader.CarryObjectMessageComposer)
        {
            WriteInteger(virtualID);
            WriteInteger(itemID);
        }
    }
}
