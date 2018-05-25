namespace Plus.HabboHotel.Quests
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using Communication.Packets.Incoming;
    using Communication.Packets.Outgoing.Inventory.Purse;
    using Communication.Packets.Outgoing.Quests;
    using Core.Logging;
    using GameClients;

    public class QuestManager
    {
        private static readonly ILogger Logger = new Logger<QuestManager>();

        private readonly Dictionary<int, Quest> _quests;
        private readonly Dictionary<string, int> _questCount;

        public QuestManager()
        {
            _quests = new Dictionary<int, Quest>();
            _questCount = new Dictionary<string, int>();

            Init();
        }

        public void Init()
        {
            if (_quests.Count > 0)
            {
                _quests.Clear();
            }

            using (var dbClient = Program.DatabaseManager.GetQueryReactor())
            {
                dbClient.SetQuery("SELECT `id`,`type`,`level_num`,`goal_type`,`goal_data`,`action`,`pixel_reward`,`data_bit`,`reward_type`,`timestamp_unlock`,`timestamp_lock` FROM `quests`");
                var dTable = dbClient.GetTable();

                if (dTable != null)
                {
                    foreach (DataRow dRow in dTable.Rows)
                    {
                        var id = Convert.ToInt32(dRow["id"]);
                        var category = Convert.ToString(dRow["type"]);
                        var num = Convert.ToInt32(dRow["level_num"]);
                        var type = Convert.ToInt32(dRow["goal_type"]);
                        var goalData = Convert.ToInt32(dRow["goal_data"]);
                        var name = Convert.ToString(dRow["action"]);
                        var reward = Convert.ToInt32(dRow["pixel_reward"]);
                        var dataBit = Convert.ToString(dRow["data_bit"]);
                        var rewardtype = Convert.ToInt32(dRow["reward_type"].ToString());
                        var time = Convert.ToInt32(dRow["timestamp_unlock"]);
                        var locked = Convert.ToInt32(dRow["timestamp_lock"]);

                        _quests.Add(id, new Quest(id, category, num, (QuestType)type, goalData, name, reward, dataBit, rewardtype, time, locked));
                        AddToCounter(category);
                    }
                }
            }

            Logger.Trace("Quest Manager -> LOADED");
        }

        private void AddToCounter(string category)
        {
            var count = 0;
            if (_questCount.TryGetValue(category, out count))
            {
                _questCount[category] = count + 1;
            }
            else
            {
                _questCount.Add(category, 1);
            }
        }

        public Quest GetQuest(int id)
        {
            _quests.TryGetValue(id, out var quest);
            return quest;
        }

        public int GetAmountOfQuestsInCategory(string category)
        {
            _questCount.TryGetValue(category, out var count);
            return count;
        }

        public void ProgressUserQuest(GameClient session, QuestType type, int data = 0)
        {
            if (session == null || session.GetHabbo() == null || session.GetHabbo().GetStats().QuestId <= 0)
            {
                return;
            }

            var quest = GetQuest(session.GetHabbo().GetStats().QuestId);

            if (quest == null || quest.GoalType != type)
            {
                return;
            }

            var currentProgress = session.GetHabbo().GetQuestProgress(quest.Id);
            var totalProgress = currentProgress;
            var completeQuest = false;

            switch (type)
            {
                default:

                    totalProgress++;

                    if (totalProgress >= quest.GoalData)
                    {
                        completeQuest = true;
                    }

                    break;

                case QuestType.ExploreFindItem:

                    if (data != quest.GoalData)
                    {
                        return;
                    }

                    totalProgress = Convert.ToInt32(quest.GoalData);
                    completeQuest = true;
                    break;

                case QuestType.StandOn:

                    if (data != quest.GoalData)
                    {
                        return;
                    }

                    totalProgress = Convert.ToInt32(quest.GoalData);
                    completeQuest = true;
                    break;

                case QuestType.XmasParty:
                    totalProgress++;
                    if (totalProgress == quest.GoalData)
                    {
                        completeQuest = true;
                    }

                    break;

                case QuestType.GiveItem:

                    if (data != quest.GoalData)
                    {
                        return;
                    }

                    totalProgress = Convert.ToInt32(quest.GoalData);
                    completeQuest = true;
                    break;
            }

            using (var dbClient = Program.DatabaseManager.GetQueryReactor())
            {
                dbClient.RunQuery("UPDATE `user_quests` SET `progress` = '" + totalProgress + "' WHERE `user_id` = '" + session.GetHabbo().Id + "' AND `quest_id` = '" + quest.Id + "' LIMIT 1");

                if (completeQuest)
                {
                    dbClient.RunQuery("UPDATE `user_stats` SET `quest_id` = '0' WHERE `id` = '" + session.GetHabbo().Id + "' LIMIT 1");
                }
            }

            session.GetHabbo().quests[session.GetHabbo().GetStats().QuestId] = totalProgress;
            session.SendPacket(new QuestStartedComposer(session, quest));

            if (completeQuest)
            {
                session.GetHabbo().GetMessenger().BroadcastAchievement(session.GetHabbo().Id, Users.Messenger.MessengerEventTypes.QuestCompleted, quest.Category + "." + quest.Name);

                session.GetHabbo().GetStats().QuestId = 0;
                session.GetHabbo().QuestLastCompleted = quest.Id;
                session.SendPacket(new QuestCompletedComposer(session, quest));
                session.GetHabbo().Duckets += quest.Reward;
                session.SendPacket(new HabboActivityPointNotificationComposer(session.GetHabbo().Duckets, quest.Reward));
                GetList(session, null);
            }
        }

        public Quest GetNextQuestInSeries(string category, int number)
        {
            foreach (var quest in _quests.Values)
            {
                if (quest.Category == category && quest.Number == number)
                {
                    return quest;
                }
            }

            return null;
        }

        public void GetList(GameClient session, ClientPacket message)
        {
            var UserQuestGoals = new Dictionary<string, int>();
            var UserQuests = new Dictionary<string, Quest>();

            foreach (var quest in _quests.Values.ToList())
            {
                if (quest.Category.Contains("xmas2012"))
                {
                    continue;
                }

                if (!UserQuestGoals.ContainsKey(quest.Category))
                {
                    UserQuestGoals.Add(quest.Category, 1);
                    UserQuests.Add(quest.Category, null);
                }

                if (quest.Number >= UserQuestGoals[quest.Category])
                {
                    var UserProgress = session.GetHabbo().GetQuestProgress(quest.Id);

                    if (session.GetHabbo().GetStats().QuestId != quest.Id && UserProgress >= quest.GoalData)
                    {
                        UserQuestGoals[quest.Category] = quest.Number + 1;
                    }
                }
            }

            foreach (var quest in _quests.Values.ToList())
            {
                foreach (var Goal in UserQuestGoals)
                {
                    if (quest.Category.Contains("xmas2012"))
                    {
                        continue;
                    }

                    if (quest.Category == Goal.Key && quest.Number == Goal.Value)
                    {
                        UserQuests[Goal.Key] = quest;
                        break;
                    }
                }
            }

            session.SendPacket(new QuestListComposer(session, message != null, UserQuests));
        }

        public void QuestReminder(GameClient session, int questId)
        {
            var Quest = GetQuest(questId);
            if (Quest == null)
            {
                return;
            }

            session.SendPacket(new QuestStartedComposer(session, Quest));
        }
    }
}