namespace Plus.Core.Settings
{
    using System.Collections.Generic;
    using System.Data;
    using Logging;

    public class SettingsManager
    {
        private readonly Dictionary<string, string> _settings;

        private static readonly ILogger Logger = new Logger<SettingsManager>();

        public SettingsManager()
        {
            _settings = new Dictionary<string, string>();
            Init();
        }

        public void Init()
        {
            _settings.Clear();

            using (var dbClient = Program.DatabaseManager.GetQueryReactor())
            {
                dbClient.SetQuery("SELECT * FROM `server_settings`");
                var table = dbClient.GetTable();

                if (table != null)
                {
                    foreach (DataRow row in table.Rows)
                    {
                        _settings.Add(row["key"].ToString().ToLower(), row["value"].ToString().ToLower());
                    }
                }
            }

            Logger.Trace("Loaded " + _settings.Count + " server settings.");
        }

        public string TryGetValue(string value)
        {
            return _settings.ContainsKey(value) ? _settings[value] : "0";
        }
    }
}
