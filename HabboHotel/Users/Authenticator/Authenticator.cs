namespace Plus.HabboHotel.Users.Authenticator
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Data;
    using UserData;

    public static class HabboFactory
    {
        public static string GetUsernameById(int UserId) // TODO: Get rid
        {
            var Name = "Unknown User";

            var Client = Program.GameContext.GetClientManager().GetClientByUserId(UserId);
            if (Client != null && Client.GetHabbo() != null)
            {
                return Client.GetHabbo().Username;
            }

            var User = Program.GameContext.GetCacheManager().GenerateUser(UserId);
            if (User != null)
            {
                return User.Username;
            }

            using (var dbClient = Program.DatabaseManager.GetQueryReactor())
            {
                dbClient.SetQuery("SELECT `username` FROM `users` WHERE `id` = @id LIMIT 1");
                dbClient.AddParameter("id", UserId);
                Name = dbClient.GetString();
            }

            if (string.IsNullOrEmpty(Name))
            {
                Name = "Unknown User";
            }

            return Name;
        }

        public static Habbo GenerateHabbo(DataRow Row, DataRow UserInfo)
        {
            return new Habbo(Convert.ToInt32(Row["id"]), Convert.ToString(Row["username"]), Convert.ToInt32(Row["rank"]), Convert.ToString(Row["motto"]), Convert.ToString(Row["look"]),
                Convert.ToString(Row["gender"]), Convert.ToInt32(Row["credits"]), Convert.ToInt32(Row["activity_points"]),
                Convert.ToInt32(Row["home_room"]), Row["block_newfriends"].ToString() == "1", Convert.ToInt32(Row["last_online"]),
                Row["hide_online"].ToString() == "1", Row["hide_inroom"].ToString() == "1",
                Convert.ToDouble(Row["account_created"]), Convert.ToInt32(Row["vip_points"]), Convert.ToString(Row["machine_id"]), Convert.ToString(Row["volume"]),
                Row["chat_preference"].ToString() == "1", Row["focus_preference"].ToString() == "1", Row["pets_muted"].ToString() == "1", Row["bots_muted"].ToString() == "1",
                Row["advertising_report_blocked"].ToString() == "1", Convert.ToDouble(Row["last_change"].ToString()), Convert.ToInt32(Row["gotw_points"]),
                Convert.ToString(Row["ignore_invites"]) == "1", Convert.ToDouble(Row["time_muted"]), Convert.ToDouble(UserInfo["trading_locked"]),
                Row["allow_gifts"].ToString() == "1", Convert.ToInt32(Row["friend_bar_state"]), Row["disable_forced_effects"].ToString() == "1",
                Row["allow_mimic"].ToString() == "1", Convert.ToInt32(Row["rank_vip"]));

        }

        private static readonly ConcurrentDictionary<int, Habbo> _usersCached = new ConcurrentDictionary<int, Habbo>();
        
        public static ICollection<Habbo> GetUsersCached() // TODO: Move this to some sort of cache controller
        {
            return _usersCached.Values;
        }

        public static bool RemoveFromCache(int Id, out Habbo Data) // TODO: Move this to some sort of cache controller
        {
            return _usersCached.TryRemove(Id, out Data);
        }

        public static Habbo GetHabboById(int UserId)
        {
            try
            {
                var Client = Program.GameContext.GetClientManager().GetClientByUserId(UserId);
                if (Client != null)
                {
                    var User = Client.GetHabbo();
                    if (User != null && User.Id > 0)
                    {
                        if (_usersCached.ContainsKey(UserId))
                        {
                            _usersCached.TryRemove(UserId, out User);
                        }

                        return User;
                    }
                }
                else
                {
                    try
                    {
                        if (_usersCached.ContainsKey(UserId))
                        {
                            return _usersCached[UserId];
                        }

                        var data = UserDataFactory.GetUserData(UserId);
                        if (data != null)
                        {
                            var Generated = data.user;
                            if (Generated != null)
                            {
                                Generated.InitInformation(data);
                                _usersCached.TryAdd(UserId, Generated);
                                return Generated;
                            }
                        }
                    }
                    catch { return null; }
                }
                return null;
            }
            catch
            {
                return null;
            }
        }

        public static Habbo GetHabboByUsername(string UserName)
        {
            try
            {
                using (var dbClient = Program.DatabaseManager.GetQueryReactor())
                {
                    dbClient.SetQuery("SELECT `id` FROM `users` WHERE `username` = @user LIMIT 1");
                    dbClient.AddParameter("user", UserName);
                    var id = dbClient.GetInteger();
                    if (id > 0)
                    {
                        return GetHabboById(Convert.ToInt32(id));
                    }
                }
                return null;
            }
            catch { return null; }
        }
    }
}