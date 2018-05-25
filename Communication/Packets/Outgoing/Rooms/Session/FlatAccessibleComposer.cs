namespace Plus.Communication.Packets.Outgoing.Rooms.Session
{
    internal class FlatAccessibleComposer : ServerPacket
    {
        public FlatAccessibleComposer(string Username)
            : base(ServerPacketHeader.FlatAccessibleMessageComposer)
        {
           WriteString(Username);
        }
    }
}
