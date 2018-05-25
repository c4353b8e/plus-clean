namespace Plus.Communication.Packets.Outgoing.Moderation
{
    internal class OpenHelpToolComposer : ServerPacket
    {
        public OpenHelpToolComposer()
            : base(ServerPacketHeader.OpenHelpToolMessageComposer)
        {
            WriteInteger(0);
        }
    }
}
