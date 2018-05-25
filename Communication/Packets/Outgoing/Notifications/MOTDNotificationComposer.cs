namespace Plus.Communication.Packets.Outgoing.Notifications
{
    internal class MotdNotificationComposer : ServerPacket
    {
        public MotdNotificationComposer(string message)
            : base(ServerPacketHeader.MotdNotificationMessageComposer)
        {
            WriteInteger(1);
            WriteString(message);
        }
    }
}
