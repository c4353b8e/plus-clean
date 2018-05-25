namespace Plus.Communication.Packets.Outgoing.Rooms.Furni.Wired
{
    internal class HideWiredConfigComposer : ServerPacket
    {
        public HideWiredConfigComposer()
            : base(ServerPacketHeader.HideWiredConfigMessageComposer)
        {
        }
    }
}