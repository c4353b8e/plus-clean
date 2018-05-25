namespace Plus.Communication.Packets.Outgoing.Rooms.Settings
{
    internal class RoomSettingsSavedComposer : ServerPacket
    {
        public RoomSettingsSavedComposer(int roomID)
            : base(ServerPacketHeader.RoomSettingsSavedMessageComposer)
        {
            WriteInteger(roomID);
        }
    }
}