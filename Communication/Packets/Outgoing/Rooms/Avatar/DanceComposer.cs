namespace Plus.Communication.Packets.Outgoing.Rooms.Avatar
{
    using HabboHotel.Rooms;

    internal class DanceComposer : ServerPacket
    {
        public DanceComposer(RoomUser Avatar, int Dance)
            : base(ServerPacketHeader.DanceMessageComposer)
        {
            WriteInteger(Avatar.VirtualId);
            WriteInteger(Dance);
        }
    }
}