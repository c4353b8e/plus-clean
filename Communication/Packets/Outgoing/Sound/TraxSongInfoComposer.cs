namespace Plus.Communication.Packets.Outgoing.Sound
{
    internal class TraxSongInfoComposer : ServerPacket
    {
        public TraxSongInfoComposer()
            : base(ServerPacketHeader.TraxSongInfoMessageComposer)
        {
            WriteInteger(0);//Count
        }
    }
}
