namespace Plus.HabboHotel.Users.Navigator.SavedSearches
{
    public class SavedSearch
    {
        public int Id { get; }
        public string Filter { get; }
        public string Search { get; }

        public SavedSearch(int id, string filter, string search)
        {
            Id = id;
            Filter = filter;
            Search = search;
        }
    }
}
