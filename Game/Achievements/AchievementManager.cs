namespace Plus.Game.Achievements
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Communication.Packets.Outgoing.Inventory.Achievements;
    using Communication.Packets.Outgoing.Inventory.Purse;
    using Core.Logging;
    using Players;

    public class AchievementManager
    {
        private static readonly ILogger Logger = new Logger<AchievementManager>();

        public Dictionary<string, Achievement> Achievements;

        public AchievementManager()
        {
            Achievements = new Dictionary<string, Achievement>();
            Init();
        }

        public void Init()
        {
            AchievementLevelFactory.GetAchievementLevels(out Achievements);
        }

        public bool ProgressAchievement(Player session, string group, int progress, bool fromBeginning = false)
        {
            if (!Achievements.ContainsKey(group) || session == null)
            {
                return false;
            }

            var data = Achievements[group];
            if (data == null)
            {
                return false;
            }

            var userData = session.GetHabbo().GetAchievementData(group);
            if (userData == null)
            {
                userData = new UserAchievement(group, 0, 0);
                session.GetHabbo().Achievements.TryAdd(group, userData);
            }

            var totalLevels = data.Levels.Count;

            if (userData.Level == totalLevels)
            {
                return false; // done, no more.
            }

            var targetLevel = userData.Level + 1;

            if (targetLevel > totalLevels)
            {
                targetLevel = totalLevels;
            }

            var level = data.Levels[targetLevel];
            int newProgress;
            if (fromBeginning)
            {
                newProgress = progress;
            }
            else
            {
                newProgress = userData.Progress + progress;
            }

            var newLevel = userData.Level;
            var newTarget = newLevel + 1;

            if (newTarget > totalLevels)
            {
                newTarget = totalLevels;
            }

            if (newProgress >= level.Requirement)
            {
                newLevel++;
                newTarget++;
                newProgress = 0;

                if (targetLevel == 1)
                {
                    session.GetHabbo().GetBadgeComponent().GiveBadge(group + targetLevel, true, session);
                }
                else
                {
                    session.GetHabbo().GetBadgeComponent().RemoveBadge(Convert.ToString(group + (targetLevel - 1)));
                    session.GetHabbo().GetBadgeComponent().GiveBadge(group + targetLevel, true, session);
                }

                if (newTarget > totalLevels)
                {
                    newTarget = totalLevels;
                }

                session.SendPacket(new AchievementUnlockedComposer(data, targetLevel, level.RewardPoints, level.RewardPixels));
                session.GetHabbo().GetMessenger().BroadcastAchievement(session.GetHabbo().Id, Users.Messenger.MessengerEventTypes.AchievementUnlocked, group + targetLevel);

                using (var dbClient = Program.DatabaseManager.GetQueryReactor())
                {
                    dbClient.SetQuery("REPLACE INTO `user_achievements` VALUES ('" + session.GetHabbo().Id + "', @group, '" + newLevel + "', '" + newProgress + "')");
                    dbClient.AddParameter("group", group);
                    dbClient.RunQuery();
                }

                userData.Level = newLevel;
                userData.Progress = newProgress;

                session.GetHabbo().Duckets += level.RewardPixels;
                session.GetHabbo().GetStats().AchievementPoints += level.RewardPoints;
                session.SendPacket(new HabboActivityPointNotificationComposer(session.GetHabbo().Duckets, level.RewardPixels));
                session.SendPacket(new AchievementScoreComposer(session.GetHabbo().GetStats().AchievementPoints));

                var newLevelData = data.Levels[newTarget];
                session.SendPacket(new AchievementProgressedComposer(data, newTarget, newLevelData, totalLevels, session.GetHabbo().GetAchievementData(group)));

                return true;
            }

            userData.Level = newLevel;
            userData.Progress = newProgress;
            using (var dbClient = Program.DatabaseManager.GetQueryReactor())
            {
                dbClient.SetQuery("REPLACE INTO `user_achievements` VALUES ('" + session.GetHabbo().Id + "', @group, '" + newLevel + "', '" + newProgress + "')");
                dbClient.AddParameter("group", group);
                dbClient.RunQuery();
            }

            session.SendPacket(new AchievementProgressedComposer(data, targetLevel, level, totalLevels, session.GetHabbo().GetAchievementData(group)));
            return false;
        }

        public ICollection<Achievement> GetGameAchievements(int gameId)
        {
            var achievements = new List<Achievement>();

            foreach (var achievement in Achievements.Values.ToList())
            {
                if (achievement.Category == "games" && achievement.GameId == gameId)
                {
                    achievements.Add(achievement);
                }
            }

            return achievements;
        }
    }
}