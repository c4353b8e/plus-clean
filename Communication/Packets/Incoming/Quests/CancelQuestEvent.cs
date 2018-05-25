namespace Plus.Communication.Packets.Incoming.Quests
{
    using Game.Players;
    using Outgoing.Quests;

    internal class CancelQuestEvent : IPacketEvent
    {
        public void Parse(Player session, ClientPacket packet)
        {
            var quest = Program.GameContext.GetQuestManager().GetQuest(session.GetHabbo().GetStats().QuestId);
            if (quest == null)
            {
                return;
            }

            using (var dbClient = Program.DatabaseManager.GetQueryReactor())
            {
                dbClient.RunQuery("DELETE FROM `user_quests` WHERE `user_id` = '" + session.GetHabbo().Id + "' AND `quest_id` = '" + quest.Id + "';" +
                    "UPDATE `user_stats` SET `quest_id` = '0' WHERE `id` = '" + session.GetHabbo().Id + "' LIMIT 1");
            }

            session.GetHabbo().GetStats().QuestId = 0;
            session.SendPacket(new QuestAbortedComposer());

            Program.GameContext.GetQuestManager().GetList(session, null);
        }
    }
}
