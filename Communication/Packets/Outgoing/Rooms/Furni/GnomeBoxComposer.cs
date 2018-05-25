namespace Plus.Communication.Packets.Outgoing.Rooms.Furni
{
    internal class GnomeBoxComposer : ServerPacket
    {
        public GnomeBoxComposer(int ItemId)
            : base(ServerPacketHeader.GnomeBoxMessageComposer)
        {
            WriteInteger(ItemId);
        }
    }
}
