namespace Plus.Communication.Packets.Outgoing.Rooms.Engine
{
    internal class UserRemoveComposer : ServerPacket
    {
        public UserRemoveComposer(int Id)
            : base(ServerPacketHeader.UserRemoveMessageComposer)
        {
           WriteString(Id.ToString());
        }
    }
}
