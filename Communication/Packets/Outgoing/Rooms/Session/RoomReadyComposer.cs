namespace Plus.Communication.Packets.Outgoing.Rooms.Session
{
    internal class RoomReadyComposer : ServerPacket
    {
        public RoomReadyComposer(int RoomId, string Model)
            : base(ServerPacketHeader.RoomReadyMessageComposer)
        {
           WriteString(Model);
            WriteInteger(RoomId);
        }
    }
}
