namespace Plus.Communication.Packets.Outgoing.Quests
{
    internal class QuestCompletedCompser : ServerPacket
    {
        public QuestCompletedCompser()
            : base(ServerPacketHeader.QuestCompletedMessageComposer)
        {

        }
    }
}
