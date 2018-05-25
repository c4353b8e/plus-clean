namespace Plus.Communication.Packets.Outgoing.Messenger
{
    internal class FindFriendsProcessResultComposer : ServerPacket
    {
        public FindFriendsProcessResultComposer(bool Found)
            : base(ServerPacketHeader.FindFriendsProcessResultMessageComposer)
        {
            WriteBoolean(Found);
        }
    }
}