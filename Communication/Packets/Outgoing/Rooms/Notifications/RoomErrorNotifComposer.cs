namespace Plus.Communication.Packets.Outgoing.Rooms.Notifications
{
    internal class RoomErrorNotifComposer : ServerPacket
    {
        public RoomErrorNotifComposer(int Error)
            : base(ServerPacketHeader.RoomErrorNotifMessageComposer)
        {
            WriteInteger(Error);
        }
    }
}
