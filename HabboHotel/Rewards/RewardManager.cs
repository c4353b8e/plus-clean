namespace Plus.HabboHotel.Rewards
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Data;
    using Communication.Packets.Outgoing.Inventory.Purse;
    using GameClients;

    public class RewardManager
    {
        private readonly ConcurrentDictionary<int, Reward> _rewards;
        private readonly ConcurrentDictionary<int, List<int>> _rewardLogs;

        public RewardManager()
        {
            _rewards = new ConcurrentDictionary<int, Reward>();
            _rewardLogs = new ConcurrentDictionary<int, List<int>>();

            Init();
        }

        public void Init()
        {
            using (var dbClient = Program.DatabaseManager.GetQueryReactor())
            {
                dbClient.SetQuery("SELECT * FROM `server_rewards` WHERE enabled = '1'");
                var dTable = dbClient.GetTable();
                if (dTable != null)
                {
                    foreach (DataRow dRow in dTable.Rows)
                    {
                        _rewards.TryAdd((int)dRow["id"], new Reward(Convert.ToDouble(dRow["reward_start"]), Convert.ToDouble(dRow["reward_end"]), Convert.ToString(dRow["reward_type"]), Convert.ToString(dRow["reward_data"]), Convert.ToString(dRow["message"])));
                    }
                }

                dbClient.SetQuery("SELECT * FROM `server_reward_logs`");
                dTable = dbClient.GetTable();
                if (dTable != null)
                {
                    foreach (DataRow dRow in dTable.Rows)
                    {
                        var Id = (int)dRow["user_id"];
                        var RewardId = (int)dRow["reward_id"];

                        if (!_rewardLogs.ContainsKey(Id))
                        {
                            _rewardLogs.TryAdd(Id, new List<int>());
                        }

                        if (!_rewardLogs[Id].Contains(RewardId))
                        {
                            _rewardLogs[Id].Add(RewardId);
                        }
                    }
                }
            }
        }

        public bool HasReward(int Id, int RewardId)
        {
            if (!_rewardLogs.ContainsKey(Id))
            {
                return false;
            }

            if (_rewardLogs[Id].Contains(RewardId))
            {
                return true;
            }

            return false;
        }

        public void LogReward(int Id, int RewardId)
        {
            if (!_rewardLogs.ContainsKey(Id))
            {
                _rewardLogs.TryAdd(Id, new List<int>());
            }

            if (!_rewardLogs[Id].Contains(RewardId))
            {
                _rewardLogs[Id].Add(RewardId);
            }

            using (var dbClient = Program.DatabaseManager.GetQueryReactor())
            {
                dbClient.SetQuery("INSERT INTO `server_reward_logs` VALUES ('', @userId, @rewardId)");
                dbClient.AddParameter("userId", Id);
                dbClient.AddParameter("rewardId", RewardId);
                dbClient.RunQuery();
            }
        }

        public void CheckRewards(GameClient Session)
        {
            if (Session == null || Session.GetHabbo() == null)
            {
                return;
            }

            foreach (var Entry in _rewards)
            {
                var Id = Entry.Key;
                var Reward = Entry.Value;

                if (HasReward(Session.GetHabbo().Id, Id))
                {
                    continue;
                }

                if (Reward.Active)
                {
                    switch (Reward.Type)
                    {
                        case RewardType.Badge:
                            {
                                if (!Session.GetHabbo().GetBadgeComponent().HasBadge(Reward.RewardData))
                                {
                                    Session.GetHabbo().GetBadgeComponent().GiveBadge(Reward.RewardData, true, Session);
                                }

                                break;
                            }

                        case RewardType.Credits:
                            {
                                Session.GetHabbo().Credits += Convert.ToInt32(Reward.RewardData);
                                Session.SendPacket(new CreditBalanceComposer(Session.GetHabbo().Credits));
                                break;
                            }

                        case RewardType.Duckets:
                            {
                                Session.GetHabbo().Duckets += Convert.ToInt32(Reward.RewardData);
                                Session.SendPacket(new HabboActivityPointNotificationComposer(Session.GetHabbo().Duckets, Convert.ToInt32(Reward.RewardData)));
                                break;
                            }

                        case RewardType.Diamonds:
                            {
                                Session.GetHabbo().Diamonds += Convert.ToInt32(Reward.RewardData);
                                Session.SendPacket(new HabboActivityPointNotificationComposer(Session.GetHabbo().Diamonds, Convert.ToInt32(Reward.RewardData), 5));
                                break;
                            }
                    }

                    if (!string.IsNullOrEmpty(Reward.Message))
                    {
                        Session.SendNotification(Reward.Message);
                    }

                    LogReward(Session.GetHabbo().Id, Id);
                }
            }
        }
    }
}
