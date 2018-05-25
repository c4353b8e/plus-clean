namespace Plus.Communication.Packets.Incoming.Quests
{
    using Game.Players;
    using Outgoing.Quests;

    internal class StartQuestEvent : IPacketEvent
    {
        public void Parse(Player session, ClientPacket packet)
        {
            var questId = packet.PopInt();

            var quest = Program.GameContext.GetQuestManager().GetQuest(questId);
            if (quest == null)
            {
                return;
            }

            using (var dbClient = Program.DatabaseManager.GetQueryReactor())
            {
                dbClient.RunQuery("REPLACE INTO `user_quests` (`user_id`,`quest_id`) VALUES ('" + session.GetHabbo().Id + "', '" + quest.Id + "')");
                dbClient.RunQuery("UPDATE `user_stats` SET `quest_id` = '" + quest.Id + "' WHERE `id` = '" + session.GetHabbo().Id + "' LIMIT 1");
            }

            session.GetHabbo().GetStats().QuestId = quest.Id;
            Program.GameContext.GetQuestManager().GetList(session, null);
            session.SendPacket(new QuestStartedComposer(session, quest));
        }
    }
}
