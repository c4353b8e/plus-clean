namespace Plus.Communication.Packets.Outgoing.Navigator
{
    internal class RoomRatingComposer : ServerPacket
    {
        public RoomRatingComposer(int score, bool canVote)
            : base(ServerPacketHeader.RoomRatingMessageComposer)
        {
            WriteInteger(score);
            WriteBoolean(canVote);
        }
    }
}
