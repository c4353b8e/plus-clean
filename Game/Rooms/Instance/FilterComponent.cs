namespace Plus.Game.Rooms.Instance
{
    using System.Text.RegularExpressions;

    public class FilterComponent
    {
        private Room _instance;

        public FilterComponent(Room Instance)
        {
            if (Instance == null)
            {
                return;
            }

            _instance = Instance;
        }

        public bool AddFilter(string Word)
        {
            if (_instance.WordFilterList.Contains(Word))
            {
                return false;
            }

            using (var dbClient = Program.DatabaseManager.GetQueryReactor())
            {
                dbClient.SetQuery("INSERT INTO `room_filter` (`room_id`,`word`) VALUES(@rid,@word);");
                dbClient.AddParameter("rid", _instance.Id);
                dbClient.AddParameter("word", Word);
                dbClient.RunQuery();
            }

            _instance.WordFilterList.Add(Word);
            return true;
        }

        public bool RemoveFilter(string Word)
        {
            if (!_instance.WordFilterList.Contains(Word))
            {
                return false;
            }

            using (var dbClient = Program.DatabaseManager.GetQueryReactor())
            {
                dbClient.SetQuery("DELETE FROM `room_filter` WHERE `room_id` = @rid AND `word` = @word;");
                dbClient.AddParameter("rid", _instance.Id);
                dbClient.AddParameter("word", Word);
                dbClient.RunQuery();
            }

            _instance.WordFilterList.Remove(Word);
            return true;
        }

        public string CheckMessage(string Message)
        {
            foreach (var Filter in _instance.WordFilterList)
            {
                if (Message.ToLower().Contains(Filter) || Message == Filter)
                {
                    Message = Regex.Replace(Message, Filter, "Bobba", RegexOptions.IgnoreCase);
                }
            }

            return Message.TrimEnd(' ');
        }

        public void Cleanup()
        {
            _instance = null;
        }
    }
}
