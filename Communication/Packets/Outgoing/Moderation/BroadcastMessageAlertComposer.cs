namespace Plus.Communication.Packets.Outgoing.Moderation
{
    internal class BroadcastMessageAlertComposer : ServerPacket
    {
        public BroadcastMessageAlertComposer(string Message, string URL = "")
            : base(ServerPacketHeader.BroadcastMessageAlertMessageComposer)
        {
           WriteString(Message);
           WriteString(URL);
        }
    }
}

