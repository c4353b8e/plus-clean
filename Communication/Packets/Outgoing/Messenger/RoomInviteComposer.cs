namespace Plus.Communication.Packets.Outgoing.Messenger
{
    internal class RoomInviteComposer : ServerPacket
    {
        public RoomInviteComposer(int SenderId, string Text)
            : base(ServerPacketHeader.RoomInviteMessageComposer)
        {
            WriteInteger(SenderId);
           WriteString(Text);
        }
    }
}
