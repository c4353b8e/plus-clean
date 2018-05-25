namespace Plus.Communication.Packets.Outgoing.Users
{
    internal class RespectNotificationComposer : ServerPacket
    {
        public RespectNotificationComposer(int userId, int respect)
            : base(ServerPacketHeader.RespectNotificationMessageComposer)
        {
            WriteInteger(userId);
            WriteInteger(respect);
        }
    }
}
