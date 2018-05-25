namespace Plus.HabboHotel.Catalog
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using Items;

    public class CatalogDeal
    {
        public int Id { get; set; }
        public List<CatalogItem> ItemDataList { get; }
        public string DisplayName { get; set; }
        public int RoomId { get; set; }

        public CatalogDeal(int id, string items, string displayName, int roomId, ItemDataManager itemDataManager)
        {
            Id = id;
            DisplayName = displayName;
            RoomId = roomId;
            ItemDataList = new List<CatalogItem>();

            if (roomId != 0)
            {
                DataTable data = null;
                using (var dbClient = Program.DatabaseManager.GetQueryReactor())
                {
                    dbClient.SetQuery("SELECT `items`.base_item, COALESCE(`items_groups`.`group_id`, 0) AS `group_id` FROM `items` LEFT OUTER JOIN `items_groups` ON `items`.`id` = `items_groups`.`id` WHERE `items`.`room_id` = @rid;");
                    dbClient.AddParameter("rid", roomId);
                    data = dbClient.GetTable();
                }

                var roomItems = new Dictionary<int, int>();
                if (data != null)
                {
                    foreach (DataRow dRow in data.Rows)
                    {
                        var item_id = Convert.ToInt32(dRow["base_item"]);
                        if (roomItems.ContainsKey(item_id))
                        {
                            roomItems[item_id]++;
                        }
                        else
                        {
                            roomItems.Add(item_id, 1);
                        }
                    }
                }

                foreach (var roomItem in roomItems)
                {
                    items += roomItem.Key + "*" + roomItem.Value + ";";
                }

                if (roomItems.Count > 0)
                {
                    items = items.Remove(items.Length - 1);
                }
            }

            var splitItems = items.Split(';');
            foreach (var split in splitItems)
            {
                var item = split.Split('*');
                if (!int.TryParse(item[0], out var itemId) || !int.TryParse(item[1], out var Amount))
                {
                    continue;
                }

                if (!itemDataManager.GetItem(itemId, out var data))
                {
                    continue;
                }

                ItemDataList.Add(new CatalogItem(0, itemId, data, string.Empty, 0, 0, 0, 0, Amount, 0, 0, false, "", "", 0));
            }
        }
    }
}
