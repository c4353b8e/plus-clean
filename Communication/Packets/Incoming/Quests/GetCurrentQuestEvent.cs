namespace Plus.Communication.Packets.Incoming.Quests
{
    using HabboHotel.GameClients;
    using Outgoing.Quests;

    internal class GetCurrentQuestEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            if (session == null || session.GetHabbo() == null || !session.GetHabbo().InRoom)
            {
                return;
            }

            var userQuest = Program.GameContext.GetQuestManager().GetQuest(session.GetHabbo().QuestLastCompleted);
            var nextQuest = Program.GameContext.GetQuestManager().GetNextQuestInSeries(userQuest.Category, userQuest.Number + 1);

            if (nextQuest == null)
            {
                return;
            }

            using (var dbClient = Program.DatabaseManager.GetQueryReactor())
            {
                dbClient.RunQuery("REPLACE INTO `user_quests`(`user_id`,`quest_id`) VALUES (" + session.GetHabbo().Id + ", " + nextQuest.Id + ")");
                dbClient.RunQuery("UPDATE `user_stats` SET `quest_id` = '" + nextQuest.Id + "' WHERE `id` = '" + session.GetHabbo().Id + "' LIMIT 1");
            }

            session.GetHabbo().GetStats().QuestId = nextQuest.Id;
            Program.GameContext.GetQuestManager().GetList(session, null);
            session.SendPacket(new QuestStartedComposer(session, nextQuest));
        }
    }
}
