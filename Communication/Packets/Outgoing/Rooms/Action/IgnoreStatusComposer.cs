namespace Plus.Communication.Packets.Outgoing.Rooms.Action
{
    internal class IgnoreStatusComposer : ServerPacket
    {
        public IgnoreStatusComposer(int Status, string Username)
            : base(ServerPacketHeader.IgnoreStatusMessageComposer)
        {
            WriteInteger(Status);
           WriteString(Username);
        }
    }
}
