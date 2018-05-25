namespace Plus.Communication.Packets.Outgoing.GameCenter
{
    internal class JoinQueueComposer : ServerPacket
    {
        public JoinQueueComposer(int GameId)
            : base(ServerPacketHeader.JoinQueueMessageComposer)
        {
            WriteInteger(GameId);
        }
    }
}
