namespace Plus.HabboHotel.Talents
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using Core.Logging;

    public class TalentTrackManager
    {
        private static readonly ILogger Logger = new Logger<TalentTrackManager>();

        private readonly Dictionary<int, TalentTrackLevel> _citizenshipLevels;

        public TalentTrackManager()
        {
            _citizenshipLevels = new Dictionary<int, TalentTrackLevel>();

            Init();
        }

        public void Init()
        {
            DataTable data = null;
            using (var dbClient = Program.DatabaseManager.GetQueryReactor())
            {
                dbClient.SetQuery("SELECT `type`,`level`,`data_actions`,`data_gifts` FROM `talents`");
                data = dbClient.GetTable();
            }

            if (data != null)
            {
                foreach (DataRow row in data.Rows)
                {
                    _citizenshipLevels.Add(Convert.ToInt32(row["level"]), new TalentTrackLevel(Convert.ToString(row["type"]), Convert.ToInt32(row["level"]), Convert.ToString(row["data_actions"]), Convert.ToString(row["data_gifts"])));
                }
            }

            Logger.Trace("Loaded " + _citizenshipLevels.Count + " talent track levels");
        }

        public ICollection<TalentTrackLevel> GetLevels()
        {
            return _citizenshipLevels.Values;
        }
    }
}
