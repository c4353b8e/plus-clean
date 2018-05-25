namespace Plus.Communication.Packets.Incoming.Quests
{
    using Game.Players;

    public class GetQuestListEvent : IPacketEvent
    {
        public void Parse(Player session, ClientPacket packet)
        {
            Program.GameContext.GetQuestManager().GetList(session, null);
        }
    }
}