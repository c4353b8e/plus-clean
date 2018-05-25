namespace Plus.Game.Cache
{
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using Core.Logging;
    using Process;
    using Type;

    public class CacheManager
    {
        private static readonly ILogger Logger = new Logger<CacheManager>();

        private readonly ConcurrentDictionary<int, UserCache> _usersCached;
        private readonly ProcessComponent _process;

        public CacheManager()
        {
            _usersCached = new ConcurrentDictionary<int, UserCache>();
            _process = new ProcessComponent();
            _process.Init();
            Logger.Trace("Cache Manager -> LOADED");
        }
        public bool ContainsUser(int id)
        {
            return _usersCached.ContainsKey(id);
        }

        public UserCache GenerateUser(int id)
        {
            UserCache user = null;

            if (_usersCached.ContainsKey(id))
            {
                if (TryGetUser(id, out user))
                {
                    return user;
                }
            }

            var client = Program.GameContext.PlayerController.GetClientByUserId(id);
            if (client != null)
            {
                if (client.GetHabbo() != null)
                {
                    user = new UserCache(id, client.GetHabbo().Username, client.GetHabbo().Motto, client.GetHabbo().Look);
                    _usersCached.TryAdd(id, user);
                    return user;
                }
            }

            using (var dbClient = Program.DatabaseManager.GetQueryReactor())
            {
                dbClient.SetQuery("SELECT `username`, `motto`, `look` FROM users WHERE id = @id LIMIT 1");
                dbClient.AddParameter("id", id);

                var dRow = dbClient.GetRow();

                if (dRow != null)
                {
                    user = new UserCache(id, dRow["username"].ToString(), dRow["motto"].ToString(), dRow["look"].ToString());
                    _usersCached.TryAdd(id, user);
                }
            }

            return user;
        }

        public bool TryRemoveUser(int id, out UserCache user)
        {
            return _usersCached.TryRemove(id, out user);
        }

        public bool TryGetUser(int id, out UserCache user)
        {
            return _usersCached.TryGetValue(id, out user);
        }

        public ICollection<UserCache> GetUserCache()
        {
            return _usersCached.Values;
        }
    }
}