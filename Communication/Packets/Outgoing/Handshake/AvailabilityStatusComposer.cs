namespace Plus.Communication.Packets.Outgoing.Handshake
{
    internal class AvailabilityStatusComposer : ServerPacket
    {
        public AvailabilityStatusComposer()
            : base(ServerPacketHeader.AvailabilityStatusMessageComposer)
        {
            WriteBoolean(true);
            WriteBoolean(false);
            WriteBoolean(true);
        }
    }
}
