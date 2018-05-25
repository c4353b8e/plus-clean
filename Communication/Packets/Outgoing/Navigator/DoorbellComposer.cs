namespace Plus.Communication.Packets.Outgoing.Navigator
{
    internal class DoorbellComposer : ServerPacket
    {
        public DoorbellComposer(string username)
            : base(ServerPacketHeader.DoorbellMessageComposer)
        {
           WriteString(username);
        }
    }
}
