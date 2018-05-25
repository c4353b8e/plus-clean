namespace Plus.Communication.Packets.Outgoing.Rooms.Settings
{
    using HabboHotel.Rooms;

    internal class FlatControllerRemovedComposer : ServerPacket
    {
        public FlatControllerRemovedComposer(Room Instance, int UserId)
            : base(ServerPacketHeader.FlatControllerRemovedMessageComposer)
        {
            WriteInteger(Instance.Id);
            WriteInteger(UserId);
        }
    }
}
