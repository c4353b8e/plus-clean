namespace Plus.HabboHotel.Items.Televisions
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using Core.Logging;

    public class TelevisionManager
    {
        private static readonly ILogger Logger = new Logger<TelevisionManager>();

        public Dictionary<int, TelevisionItem> _televisions;

        public TelevisionManager()
        {
            _televisions =  new Dictionary<int, TelevisionItem>();
        }

        public void Init()
        {
            if (_televisions.Count > 0)
            {
                _televisions.Clear();
            }

            DataTable getData = null;
            using (var dbClient = Program.DatabaseManager.GetQueryReactor()) 
            {
                dbClient.SetQuery("SELECT * FROM `items_youtube` ORDER BY `id` DESC");
                getData = dbClient.GetTable();

                if (getData != null)
                {
                    foreach (DataRow Row in getData.Rows)
                    {
                        _televisions.Add(Convert.ToInt32(Row["id"]), new TelevisionItem(Convert.ToInt32(Row["id"]), Row["youtube_id"].ToString(), Row["title"].ToString(), Row["description"].ToString(), Row["enabled"].ToString() == "1"));
                    }
                }
            }
        }


        public ICollection<TelevisionItem> TelevisionList => _televisions.Values;

        public bool TryGet(int ItemId, out TelevisionItem TelevisionItem)
        {
            if (_televisions.TryGetValue(ItemId, out TelevisionItem))
            {
                return true;
            }

            return false;
        }
    }
}
