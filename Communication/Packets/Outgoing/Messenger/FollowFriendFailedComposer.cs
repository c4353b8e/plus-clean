namespace Plus.Communication.Packets.Outgoing.Messenger
{
    internal class FollowFriendFailedComposer : ServerPacket
    {
        public FollowFriendFailedComposer(int ErrorCode)
            : base(ServerPacketHeader.FollowFriendFailedMessageComposer)
        {
            WriteInteger(ErrorCode);
        }
    }
}
