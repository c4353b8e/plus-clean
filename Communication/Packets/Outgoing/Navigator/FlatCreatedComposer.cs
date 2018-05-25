namespace Plus.Communication.Packets.Outgoing.Navigator
{
    internal class FlatCreatedComposer : ServerPacket
    {
        public FlatCreatedComposer(int roomId, string roomName)
            : base(ServerPacketHeader.FlatCreatedMessageComposer)
        {
            WriteInteger(roomId);
            WriteString(roomName);
        }
    }
}
