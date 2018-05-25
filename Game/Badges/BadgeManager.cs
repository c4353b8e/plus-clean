namespace Plus.Game.Badges
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using Core.Logging;

    public class BadgeManager
    {
        private static readonly ILogger Logger = new Logger<BadgeManager>();

        private readonly Dictionary<string, BadgeDefinition> _badges;

        public BadgeManager()
        {
            _badges = new Dictionary<string, BadgeDefinition>();
            Init();
        }

        public void Init()
        {
            using (var dbClient = Program.DatabaseManager.GetQueryReactor())
            {
                dbClient.SetQuery("SELECT * FROM `badge_definitions`;");
                var data = dbClient.GetTable();

                foreach (DataRow row in data.Rows)
                {
                    var code = Convert.ToString(row["code"]).ToUpper();

                    if (!_badges.ContainsKey(code))
                    {
                        _badges.Add(code, new BadgeDefinition(code, Convert.ToString(row["required_right"])));
                    }
                }
            }

            Logger.Trace("Loaded " + _badges.Count + " badge definitions.");
        }
   
        public bool TryGetBadge(string code, out BadgeDefinition badge)
        {
            return _badges.TryGetValue(code.ToUpper(), out badge);
        }
    }
}