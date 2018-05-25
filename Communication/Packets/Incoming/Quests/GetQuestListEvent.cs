namespace Plus.Communication.Packets.Incoming.Quests
{
    using HabboHotel.GameClients;

    public class GetQuestListEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            Program.GameContext.GetQuestManager().GetList(session, null);
        }
    }
}