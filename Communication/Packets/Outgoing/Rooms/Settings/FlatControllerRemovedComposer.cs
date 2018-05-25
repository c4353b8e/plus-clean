namespace Plus.Communication.Packets.Outgoing.Rooms.Settings
{
    using Game.Rooms;

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
