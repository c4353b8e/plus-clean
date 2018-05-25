namespace Plus.Communication.Packets.Outgoing.Handshake
{
    internal class GenericErrorComposer : ServerPacket
    {
        public GenericErrorComposer(int errorId)
            : base(ServerPacketHeader.GenericErrorMessageComposer)
        {
            WriteInteger(errorId);
        }
    }
}
