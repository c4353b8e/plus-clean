namespace Plus.Communication.Packets.Outgoing.Rooms.Permissions
{
    internal class YouAreControllerComposer : ServerPacket
    {
        public YouAreControllerComposer(int Setting)
            : base(ServerPacketHeader.YouAreControllerMessageComposer)
        {
            WriteInteger(Setting);
        }
    }
}
