namespace Plus.Communication.Packets.Outgoing.Groups
{
    internal class NewGroupInfoComposer : ServerPacket
    {
        public NewGroupInfoComposer(int RoomId, int GroupId)
            : base(ServerPacketHeader.NewGroupInfoMessageComposer)
        {
            WriteInteger(RoomId);
            WriteInteger(GroupId);
        }
    }
}
