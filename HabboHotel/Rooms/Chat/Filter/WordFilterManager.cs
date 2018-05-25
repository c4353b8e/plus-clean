namespace Plus.HabboHotel.Rooms.Chat.Filter
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using System.Text.RegularExpressions;

    public sealed class WordFilterManager
    {
        private readonly List<WordFilter> _filteredWords;

        public WordFilterManager()
        {
            _filteredWords = new List<WordFilter>();
        }

        public void Init()
        {
            if (_filteredWords.Count > 0)
            {
                _filteredWords.Clear();
            }

            DataTable data = null;
            using (var dbClient = Program.DatabaseManager.GetQueryReactor())
            {
                dbClient.SetQuery("SELECT * FROM `wordfilter`");
                data = dbClient.GetTable();

                if (data != null)
                {
                    foreach (DataRow Row in data.Rows)
                    {
                        _filteredWords.Add(new WordFilter(Convert.ToString(Row["word"]), Convert.ToString(Row["replacement"]), Row["strict"].ToString() == "1", Row["bannable"].ToString() == "1"));
                    }
                }
            }
        }

        public string CheckMessage(string message)
        {
            foreach (var Filter in _filteredWords.ToList())
            {
                if (message.ToLower().Contains(Filter.Word) && Filter.IsStrict || message == Filter.Word)
                {
                    message = Regex.Replace(message, Filter.Word, Filter.Replacement, RegexOptions.IgnoreCase);
                }
                else if (message.ToLower().Contains(Filter.Word) && !Filter.IsStrict || message == Filter.Word)
                {
                    var Words = message.Split(' ');

                    message = "";
                    foreach (var Word in Words.ToList())
                    {
                        if (Word.ToLower() == Filter.Word)
                        {
                            message += Filter.Replacement + " ";
                        }
                        else
                        {
                            message += Word + " ";
                        }
                    }
                }
            }

            return message.TrimEnd(' ');
        }

        public bool CheckBannedWords(string message)
        {
            message = message.Replace(" ", "").Replace(".", "").Replace("_", "").ToLower();

            foreach (var Filter in _filteredWords.ToList())
            {
                if (!Filter.IsBannable)
                {
                    continue;
                }

                if (message.Contains(Filter.Word))
                {
                    return true;
                }
            }
            return false;
        }

        public bool IsFiltered(string message)
        {
            foreach (var filter in _filteredWords.ToList())
            {
                if (message.Contains(filter.Word))
                {
                    return true;
                }
            }
            return false;
        }
    }
}