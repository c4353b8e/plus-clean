namespace Plus.Communication.Packets.Outgoing.Rooms.Polls
{
    internal class PollOfferComposer : ServerPacket
    {
        public PollOfferComposer(int roomId) : base(1074)
        {
            WriteInteger(roomId);//Room Id
            WriteString("CLIENT_NPS");
            WriteString("Customer Satisfaction Poll");
            WriteString("Give us your opinion!");
        }
    }
}
