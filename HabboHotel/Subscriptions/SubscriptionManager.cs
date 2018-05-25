namespace Plus.HabboHotel.Subscriptions
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using Core.Logging;

    public class SubscriptionManager
    {
        private static readonly ILogger Logger = new Logger<SubscriptionManager>();

        private readonly Dictionary<int, SubscriptionData> _subscriptions = new Dictionary<int, SubscriptionData>();

        public SubscriptionManager()
        {
            Init();
        }

        public void Init()
        {
            if (_subscriptions.Count > 0)
            {
                _subscriptions.Clear();
            }

            using (var dbClient = Program.DatabaseManager.GetQueryReactor())
            {
                dbClient.SetQuery("SELECT * FROM `subscriptions`;");
                var GetSubscriptions = dbClient.GetTable();

                if (GetSubscriptions != null)
                {
                    foreach (DataRow Row in GetSubscriptions.Rows)
                    {
                        if (!_subscriptions.ContainsKey(Convert.ToInt32(Row["id"])))
                        {
                            _subscriptions.Add(Convert.ToInt32(Row["id"]), new SubscriptionData(Convert.ToInt32(Row["id"]), Convert.ToString(Row["name"]), Convert.ToString(Row["badge_code"]), Convert.ToInt32(Row["credits"]), Convert.ToInt32(Row["duckets"]), Convert.ToInt32(Row["respects"])));
                        }
                    }
                }
            }

            Logger.Trace("Loaded " + _subscriptions.Count + " subscriptions.");
        }

        public bool TryGetSubscriptionData(int Id, out SubscriptionData Data)
        {
            return _subscriptions.TryGetValue(Id, out Data);
        }
    }
}
