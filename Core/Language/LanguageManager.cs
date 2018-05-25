namespace Plus.Core.Language
{
    using System.Collections.Generic;
    using System.Data;
    using Logging;

    public class LanguageManager
    {
        private readonly Dictionary<string, string> _values;

        private static readonly ILogger Logger = new Logger<LanguageManager>();

        public LanguageManager()
        {
            _values = new Dictionary<string, string>();
            Init();
        }

        public void Init()
        {
            _values.Clear();

            using (var dbClient = Program.DatabaseManager.GetQueryReactor())
            {
                dbClient.SetQuery("SELECT * FROM `server_locale`");
                var Table = dbClient.GetTable();

                if (Table != null)
                {
                    foreach (DataRow Row in Table.Rows)
                    {
                        _values.Add(Row["key"].ToString(), Row["value"].ToString());
                    }
                }
            }

            Logger.Trace("Loaded " + _values.Count + " language locales.");
        }

        public string TryGetValue(string value)
        {
            return _values.ContainsKey(value) ? _values[value] : "No language locale found for [" + value + "]";
        }
    }
}
