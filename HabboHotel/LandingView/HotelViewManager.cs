namespace Plus.HabboHotel.LandingView
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using Core.Logging;
    using Promotions;

    public class HotelViewManager
    {
        private static readonly ILogger Logger = new Logger<HotelViewManager>();

        private readonly Dictionary<int, Promotion> _promotionItems;

        public HotelViewManager()
        {
            _promotionItems = new Dictionary<int, Promotion>();

            Init();
        }

        public void Init()
        {
            if (_promotionItems.Count > 0)
            {
                _promotionItems.Clear();
            }

            using (var dbClient = Program.DatabaseManager.GetQueryReactor())
            {
                dbClient.SetQuery("SELECT * FROM `server_landing` ORDER BY `id` DESC");
                var GetData = dbClient.GetTable();

                if (GetData != null)
                {
                    foreach (DataRow Row in GetData.Rows)
                    {
                        _promotionItems.Add(Convert.ToInt32(Row[0]), new Promotion((int)Row[0], Row[1].ToString(), Row[2].ToString(), Row[3].ToString(), Convert.ToInt32(Row[4]), Row[5].ToString(), Row[6].ToString()));
                    }
                }
            }


            Logger.Trace("Landing View Manager -> LOADED");
        }

        public ICollection<Promotion> GetPromotionItems()
        {
            return _promotionItems.Values;
        }
    }
}