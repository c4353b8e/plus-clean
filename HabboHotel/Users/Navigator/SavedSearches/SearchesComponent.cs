namespace Plus.HabboHotel.Users.Navigator.SavedSearches
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Data;

    public class SearchesComponent
    {
        private readonly ConcurrentDictionary<int, SavedSearch> _savedSearches;

        public SearchesComponent()
        {
            _savedSearches = new ConcurrentDictionary<int, SavedSearch>();
        }

        public bool Init(Habbo habbo)
        {
            if (_savedSearches.Count > 0)
            {
                _savedSearches.Clear();
            }

            DataTable GetSearches = null;
            using (var dbClient = Program.DatabaseManager.GetQueryReactor())
            {
                dbClient.SetQuery("SELECT `id`,`filter`,`search_code` FROM `user_saved_searches` WHERE `user_id` = @UserId");
                dbClient.AddParameter("UserId", habbo.Id);
                GetSearches = dbClient.GetTable();

                if (GetSearches != null)
                {
                    foreach (DataRow Row in GetSearches.Rows)
                    {
                        _savedSearches.TryAdd(Convert.ToInt32(Row["id"]), new SavedSearch(Convert.ToInt32(Row["id"]), Convert.ToString(Row["filter"]), Convert.ToString(Row["search_code"])));
                    }
                }
            }
            return true;
        }

        public ICollection<SavedSearch> Searches => _savedSearches.Values;
    }
}
