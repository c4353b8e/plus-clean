namespace Plus.HabboHotel.Rooms.Instance
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Data;
    using Utilities;

    public class BansComponent
    {
        private Room _instance;

        private ConcurrentDictionary<int, double> _bans;

        public BansComponent(Room Instance)
        {
            if (Instance == null)
            {
                return;
            }

            _instance = Instance;
            _bans = new ConcurrentDictionary<int, double>();

            DataTable GetBans = null;
            using (var dbClient = Program.DatabaseManager.GetQueryReactor())
            {
                dbClient.SetQuery("SELECT `user_id`, `expire` FROM `room_bans` WHERE `room_id` = " + _instance.Id + " AND `expire` > UNIX_TIMESTAMP();");
                GetBans = dbClient.GetTable();

                if (GetBans != null)
                {
                    foreach (DataRow Row in GetBans.Rows)
                    {
                        _bans.TryAdd(Convert.ToInt32(Row["user_id"]), Convert.ToDouble(Row["expire"]));
                    }
                }
            }
        }

        public void Ban(RoomUser Avatar, double Time)
        {
            if (Avatar == null || _instance.CheckRights(Avatar.GetClient(), true) || IsBanned(Avatar.UserId))
            {
                return;
            }

            var BanTime = UnixTimestamp.GetNow() + Time;
            if (!_bans.TryAdd(Avatar.UserId, BanTime))
            {
                _bans[Avatar.UserId] = BanTime;
            }

            using (var dbClient = Program.DatabaseManager.GetQueryReactor())
            {
                dbClient.SetQuery("REPLACE INTO `room_bans` (`user_id`,`room_id`,`expire`) VALUES (@uid, @rid, @expire);");
                dbClient.AddParameter("rid", _instance.Id);
                dbClient.AddParameter("uid", Avatar.UserId);
                dbClient.AddParameter("expire", BanTime);
                dbClient.RunQuery();
            }

            _instance.GetRoomUserManager().RemoveUserFromRoom(Avatar.GetClient(), true, true);
        }

        public bool IsBanned(int UserId)
        {
            if (!_bans.ContainsKey(UserId))
            {
                return false;
            }

            var BanTime = _bans[UserId] - UnixTimestamp.GetNow();
            if (BanTime <= 0)
            {
                double time;
                _bans.TryRemove(UserId, out time);

                using (var dbClient = Program.DatabaseManager.GetQueryReactor())
                {
                    dbClient.SetQuery("DELETE FROM `room_bans` WHERE `room_id` = @rid AND `user_id` = @uid;");
                    dbClient.AddParameter("rid", _instance.Id);
                    dbClient.AddParameter("uid", UserId);
                    dbClient.RunQuery();
                }
                return false;
            }

            return true;
        }

        public bool Unban(int UserId)
        {
            if (!_bans.ContainsKey(UserId))
            {
                return false;
            }

            if (_bans.TryRemove(UserId, out var time))
            {
                using (var dbClient = Program.DatabaseManager.GetQueryReactor())
                {
                    dbClient.SetQuery("DELETE FROM `room_bans` WHERE `room_id` = @rid AND `user_id` = @uid;");
                    dbClient.AddParameter("rid", _instance.Id);
                    dbClient.AddParameter("uid", UserId);
                    dbClient.RunQuery();
                }
                return true;
            }

            return false;
        }

        public List<int> BannedUsers()
        {
            DataTable GetBans = null;
            var Bans = new List<int>();

            using (var dbClient = Program.DatabaseManager.GetQueryReactor())
            {
                dbClient.SetQuery("SELECT `user_id` FROM `room_bans` WHERE `room_id` = '" + _instance.Id + "' AND `expire` > UNIX_TIMESTAMP();");
                GetBans = dbClient.GetTable();

                if (GetBans != null)
                {
                    foreach (DataRow Row in GetBans.Rows)
                    {
                        if (!Bans.Contains(Convert.ToInt32(Row["user_id"])))
                        {
                            Bans.Add(Convert.ToInt32(Row["user_id"]));
                        }
                    }
                }
            }

            return Bans;
        }

        public int Count => _bans.Count;

        public void Cleanup()
        {
            _bans.Clear();

            _instance = null;
            _bans = null;
        }
    }
}