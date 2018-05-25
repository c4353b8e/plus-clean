namespace Plus.Communication.Packets.Outgoing.Navigator
{
    internal class CanCreateRoomComposer : ServerPacket
    {
        public CanCreateRoomComposer(bool error, int maxRoomsPerUser)
            : base(ServerPacketHeader.CanCreateRoomMessageComposer)
        {
            WriteInteger(error ? 1 : 0);
            WriteInteger(maxRoomsPerUser);
        }
    }
}
