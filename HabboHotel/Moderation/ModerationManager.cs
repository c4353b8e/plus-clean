namespace Plus.HabboHotel.Moderation
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using Core.Logging;
    using Utilities;

    public sealed class ModerationManager
    {
        private static readonly ILogger Logger = new Logger<ModerationManager>();

        private int _ticketCount = 1;
        private readonly List<string> _userPresets = new List<string>();
        private readonly List<string> _roomPresets = new List<string>();
        private readonly Dictionary<string, ModerationBan> _bans = new Dictionary<string, ModerationBan>();
        private readonly Dictionary<int, string> _userActionPresetCategories = new Dictionary<int, string>();
        private readonly Dictionary<int, List<ModerationPresetActionMessages>> _userActionPresetMessages = new Dictionary<int, List<ModerationPresetActionMessages>>();
        private readonly ConcurrentDictionary<int, ModerationTicket> _modTickets = new ConcurrentDictionary<int, ModerationTicket>();


        private readonly Dictionary<int, string> _moderationCFHTopics = new Dictionary<int, string>();
        private readonly Dictionary<int, List<ModerationPresetActions>> _moderationCFHTopicActions = new Dictionary<int, List<ModerationPresetActions>>();

        public ModerationManager()
        {
            Init();
        }

        public void Init()
        {
            if (_userPresets.Count > 0)
            {
                _userPresets.Clear();
            }

            if (_moderationCFHTopics.Count > 0)
            {
                _moderationCFHTopics.Clear();
            }

            if (_moderationCFHTopicActions.Count > 0)
            {
                _moderationCFHTopicActions.Clear();
            }

            if (_bans.Count > 0)
            {
                _bans.Clear();
            }

            using (var dbClient = Program.DatabaseManager.GetQueryReactor())
            {
                DataTable PresetsTable = null;
                dbClient.SetQuery("SELECT * FROM `moderation_presets`;");
                PresetsTable = dbClient.GetTable();

                if (PresetsTable != null)
                {
                    foreach (DataRow Row in PresetsTable.Rows)
                    {
                        var Type = Convert.ToString(Row["type"]).ToLower();
                        switch (Type)
                        {
                            case "user":
                                _userPresets.Add(Convert.ToString(Row["message"]));
                                break;

                            case "room":
                                _roomPresets.Add(Convert.ToString(Row["message"]));
                                break;
                        }
                    }
                }
            }

            using (var dbClient = Program.DatabaseManager.GetQueryReactor())
            {
                DataTable ModerationTopics = null;
                dbClient.SetQuery("SELECT * FROM `moderation_topics`;");
                ModerationTopics = dbClient.GetTable();

                if (ModerationTopics != null)
                {
                    foreach (DataRow Row in ModerationTopics.Rows)
                    {
                        if (!_moderationCFHTopics.ContainsKey(Convert.ToInt32(Row["id"])))
                        {
                            _moderationCFHTopics.Add(Convert.ToInt32(Row["id"]), Convert.ToString(Row["caption"]));
                        }
                    }
                }
            }

            using (var dbClient = Program.DatabaseManager.GetQueryReactor())
            {
                DataTable ModerationTopicsActions = null;
                dbClient.SetQuery("SELECT * FROM `moderation_topic_actions`;");
                ModerationTopicsActions = dbClient.GetTable();

                if (ModerationTopicsActions != null)
                {
                    foreach (DataRow Row in ModerationTopicsActions.Rows)
                    {
                        var ParentId = Convert.ToInt32(Row["parent_id"]);

                        if (!_moderationCFHTopicActions.ContainsKey(ParentId))
                        {
                            _moderationCFHTopicActions.Add(ParentId, new List<ModerationPresetActions>());
                        }

                        _moderationCFHTopicActions[ParentId].Add(new ModerationPresetActions(Convert.ToInt32(Row["id"]), Convert.ToInt32(Row["parent_id"]), Convert.ToString(Row["type"]), Convert.ToString(Row["caption"]), Convert.ToString(Row["message_text"]),
                            Convert.ToInt32(Row["mute_time"]), Convert.ToInt32(Row["ban_time"]), Convert.ToInt32(Row["ip_time"]), Convert.ToInt32(Row["trade_lock_time"]), Convert.ToString(Row["default_sanction"])));
                    }
                }
            }

            using (var dbClient = Program.DatabaseManager.GetQueryReactor())
            {
                DataTable PresetsActionCats = null;
                dbClient.SetQuery("SELECT * FROM `moderation_preset_action_categories`;");
                PresetsActionCats = dbClient.GetTable();

                if (PresetsActionCats != null)
                {
                    foreach (DataRow Row in PresetsActionCats.Rows)
                    {
                        _userActionPresetCategories.Add(Convert.ToInt32(Row["id"]), Convert.ToString(Row["caption"]));
                    }
                }
            }

            using (var dbClient = Program.DatabaseManager.GetQueryReactor())
            {
                DataTable PresetsActionMessages = null;
                dbClient.SetQuery("SELECT * FROM `moderation_preset_action_messages`;");
                PresetsActionMessages = dbClient.GetTable();

                if (PresetsActionMessages != null)
                {
                    foreach (DataRow Row in PresetsActionMessages.Rows)
                    {
                        var ParentId = Convert.ToInt32(Row["parent_id"]);

                        if (!_userActionPresetMessages.ContainsKey(ParentId))
                        {
                            _userActionPresetMessages.Add(ParentId, new List<ModerationPresetActionMessages>());
                        }

                        _userActionPresetMessages[ParentId].Add(new ModerationPresetActionMessages(Convert.ToInt32(Row["id"]), Convert.ToInt32(Row["parent_id"]), Convert.ToString(Row["caption"]), Convert.ToString(Row["message_text"]),
                            Convert.ToInt32(Row["mute_hours"]), Convert.ToInt32(Row["ban_hours"]), Convert.ToInt32(Row["ip_ban_hours"]), Convert.ToInt32(Row["trade_lock_days"]), Convert.ToString(Row["notice"])));
                    }
                }
            }

            using (var dbClient = Program.DatabaseManager.GetQueryReactor())
            {
                DataTable GetBans = null;
                dbClient.SetQuery("SELECT `bantype`,`value`,`reason`,`expire` FROM `bans` WHERE `bantype` = 'machine' OR `bantype` = 'user'");
                GetBans = dbClient.GetTable();

                if (GetBans != null)
                {
                    foreach (DataRow dRow in GetBans.Rows)
                    {
                        var value = Convert.ToString(dRow["value"]);
                        var reason = Convert.ToString(dRow["reason"]);
                        var expires = (double)dRow["expire"];
                        var type = Convert.ToString(dRow["bantype"]);

                        var Ban = new ModerationBan(BanTypeUtility.GetModerationBanType(type), value, reason, expires);
                        if (Ban != null)
                        {
                            if (expires > UnixTimestamp.GetNow())
                            {
                                if (!_bans.ContainsKey(value))
                                {
                                    _bans.Add(value, Ban);
                                }
                            }
                            else
                            {
                                dbClient.SetQuery("DELETE FROM `bans` WHERE `bantype` = '" + BanTypeUtility.FromModerationBanType(Ban.Type) + "' AND `value` = @Key LIMIT 1");
                                dbClient.AddParameter("Key", value);
                                dbClient.RunQuery();
                            }
                        }
                    }
                }
            }

            Logger.Trace("Loaded moderation manager");
        }

        public void ReCacheBans()
        {
            if (_bans.Count > 0)
            {
                _bans.Clear();
            }

            using (var dbClient = Program.DatabaseManager.GetQueryReactor())
            {
                DataTable GetBans = null;
                dbClient.SetQuery("SELECT `bantype`,`value`,`reason`,`expire` FROM `bans` WHERE `bantype` = 'machine' OR `bantype` = 'user'");
                GetBans = dbClient.GetTable();

                if (GetBans != null)
                {
                    foreach (DataRow dRow in GetBans.Rows)
                    {
                        var value = Convert.ToString(dRow["value"]);
                        var reason = Convert.ToString(dRow["reason"]);
                        var expires = (double)dRow["expire"];
                        var type = Convert.ToString(dRow["bantype"]);

                        var Ban = new ModerationBan(BanTypeUtility.GetModerationBanType(type), value, reason, expires);
                        if (Ban != null)
                        {
                            if (expires > UnixTimestamp.GetNow())
                            {
                                if (!_bans.ContainsKey(value))
                                {
                                    _bans.Add(value, Ban);
                                }
                            }
                            else
                            {
                                dbClient.SetQuery("DELETE FROM `bans` WHERE `bantype` = '" + BanTypeUtility.FromModerationBanType(Ban.Type) + "' AND `value` = @Key LIMIT 1");
                                dbClient.AddParameter("Key", value);
                                dbClient.RunQuery();
                            }
                        }
                    }
                }
            }

            Logger.Trace("Cached " + _bans.Count + " username and machine bans.");
        }

        public void BanUser(string Mod, ModerationBanType Type, string BanValue, string Reason, double ExpireTimestamp)
        {
            var BanType = Type == ModerationBanType.IP ? "ip" : Type == ModerationBanType.Machine ? "machine" : "user";
            using (var dbClient = Program.DatabaseManager.GetQueryReactor())
            {
                dbClient.SetQuery("REPLACE INTO `bans` (`bantype`, `value`, `reason`, `expire`, `added_by`,`added_date`) VALUES ('" + BanType + "', '" + BanValue + "', @reason, " + ExpireTimestamp + ", '" + Mod + "', '" + UnixTimestamp.GetNow() + "');");
                dbClient.AddParameter("reason", Reason);
                dbClient.RunQuery();
            }

            if (Type == ModerationBanType.Machine || Type == ModerationBanType.Username)
            {
                if (!_bans.ContainsKey(BanValue))
                {
                    _bans.Add(BanValue, new ModerationBan(Type, BanValue, Reason, ExpireTimestamp));
                }
            }
        }

        public ICollection<string> UserMessagePresets => _userPresets;

        public ICollection<string> RoomMessagePresets => _roomPresets;

        public ICollection<ModerationTicket> GetTickets => _modTickets.Values;

        public Dictionary<string, List<ModerationPresetActions>> UserActionPresets
        {
            get
            {
                var Result = new Dictionary<string, List<ModerationPresetActions>>();
                foreach (var Category in _moderationCFHTopics.ToList())
                {
                    Result.Add(Category.Value, new List<ModerationPresetActions>());

                    if (_moderationCFHTopicActions.ContainsKey(Category.Key))
                    {
                        foreach (var Data in _moderationCFHTopicActions[Category.Key])
                        {
                            Result[Category.Value].Add(Data);
                        }
                    }
                }
                return Result;
            }
        }

        public bool TryAddTicket(ModerationTicket Ticket)
        {
            Ticket.Id = _ticketCount++;
            return _modTickets.TryAdd(Ticket.Id, Ticket);
        }

        public bool TryGetTicket(int TicketId, out ModerationTicket Ticket)
        {
            return _modTickets.TryGetValue(TicketId, out Ticket);
        }

        public bool UserHasTickets(int userId)
        {
            return _modTickets.Count(x => x.Value.Sender.Id == userId && x.Value.Answered == false) > 0;
        }

        public ModerationTicket GetTicketBySenderId(int userId)
        {
            return _modTickets.FirstOrDefault(x => x.Value.Sender.Id == userId).Value;
        }

        public bool IsBanned(string Key, out ModerationBan Ban)
        {
            if (_bans.TryGetValue(Key, out Ban))
            {
                if (!Ban.Expired)
                {
                    return true;
                }

                //This ban has expired, let us quickly remove it here.
                using (var dbClient = Program.DatabaseManager.GetQueryReactor())
                {
                    dbClient.SetQuery("DELETE FROM `bans` WHERE `bantype` = '" + BanTypeUtility.FromModerationBanType(Ban.Type) + "' AND `value` = @Key LIMIT 1");
                    dbClient.AddParameter("Key", Key);
                    dbClient.RunQuery();
                }

                //And finally, let us remove the ban record from the cache.
                if (_bans.ContainsKey(Key))
                {
                    _bans.Remove(Key);
                }

                return false;
            }
            return false;
        }

        public bool MachineBanCheck(string MachineId)
        {
            ModerationBan MachineBanRecord = null;
            if (Program.GameContext.GetModerationManager().IsBanned(MachineId, out MachineBanRecord))
            {
                DataRow BanRow = null;
                using (var dbClient = Program.DatabaseManager.GetQueryReactor())
                {
                    dbClient.SetQuery("SELECT * FROM `bans` WHERE `bantype` = 'machine' AND `value` = @value LIMIT 1");
                    dbClient.AddParameter("value", MachineId);
                    BanRow = dbClient.GetRow();

                    //If there is no more ban record, then we can simply remove it from our cache!
                    if (BanRow == null)
                    {
                        Program.GameContext.GetModerationManager().RemoveBan(MachineId);
                        return false;
                    }
                }
            }
            return true;
        }
       
        public bool UsernameBanCheck(string Username)
        {
            ModerationBan UsernameBanRecord = null;
            if (Program.GameContext.GetModerationManager().IsBanned(Username, out UsernameBanRecord))
            {
                DataRow BanRow = null;
                using (var dbClient = Program.DatabaseManager.GetQueryReactor())
                {
                    dbClient.SetQuery("SELECT * FROM `bans` WHERE `bantype` = 'user' AND `value` = @value LIMIT 1");
                    dbClient.AddParameter("value", Username);
                    BanRow = dbClient.GetRow();

                    //If there is no more ban record, then we can simply remove it from our cache!
                    if (BanRow == null)
                    {
                        Program.GameContext.GetModerationManager().RemoveBan(Username);
                        return false;
                    }
                }
            }
            return true;
        }

        public void RemoveBan(string Value)
        {
            _bans.Remove(Value);
        }
    }
}
