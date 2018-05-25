namespace Plus.Communication.Packets.Outgoing.Rooms.Settings
{
    internal class UnbanUserFromRoomComposer : ServerPacket
    {
        public UnbanUserFromRoomComposer(int RoomId, int UserId)
            : base(ServerPacketHeader.UnbanUserFromRoomMessageComposer)
        {
            WriteInteger(RoomId);
            WriteInteger(UserId);
        }
    }
}
