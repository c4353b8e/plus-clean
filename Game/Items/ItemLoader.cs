namespace Plus.Game.Items
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using Rooms;

    public static class ItemLoader
    {
        public static List<Item> GetItemsForRoom(int roomId, Room room)
        {
            var items = new List<Item>();

            using (var dbClient = Program.DatabaseManager.GetQueryReactor())
            {
                dbClient.SetQuery("SELECT `items`.*, COALESCE(`items_groups`.`group_id`, 0) AS `group_id` FROM `items` LEFT OUTER JOIN `items_groups` ON `items`.`id` = `items_groups`.`id` WHERE `items`.`room_id` = @rid;");
                dbClient.AddParameter("rid", roomId);
                var data = dbClient.GetTable();

                if (data != null)
                {
                    foreach (DataRow row in data.Rows)
                    {
                        if (Program.GameContext.GetItemManager().GetItem(Convert.ToInt32(row["base_item"]), out var Data))
                        {
                            items.Add(new Item(Convert.ToInt32(row["id"]), Convert.ToInt32(row["room_id"]), Convert.ToInt32(row["base_item"]), Convert.ToString(row["extra_data"]),
                                Convert.ToInt32(row["x"]), Convert.ToInt32(row["y"]), Convert.ToDouble(row["z"]), Convert.ToInt32(row["rot"]), Convert.ToInt32(row["user_id"]), 
                                Convert.ToInt32(row["group_id"]), Convert.ToInt32(row["limited_number"]), Convert.ToInt32(row["limited_stack"]), Convert.ToString(row["wall_pos"]), room));
                        }
                    }
                }
            }
            return items;
        }

        public static List<Item> GetItemsForUser(int UserId)
        {
            DataTable Items = null;
            var I = new List<Item>();

            using (var dbClient = Program.DatabaseManager.GetQueryReactor())
            {
                dbClient.SetQuery("SELECT `items`.*, COALESCE(`items_groups`.`group_id`, 0) AS `group_id` FROM `items` LEFT OUTER JOIN `items_groups` ON `items`.`id` = `items_groups`.`id` WHERE `items`.`room_id` = 0 AND `items`.`user_id` = @uid;");
                dbClient.AddParameter("uid", UserId);
                Items = dbClient.GetTable();

                if (Items != null)
                {
                    foreach (DataRow Row in Items.Rows)
                    {
                        if (Program.GameContext.GetItemManager().GetItem(Convert.ToInt32(Row["base_item"]), out var Data))
                        {
                            I.Add(new Item(Convert.ToInt32(Row["id"]), Convert.ToInt32(Row["room_id"]), Convert.ToInt32(Row["base_item"]), Convert.ToString(Row["extra_data"]),
                                Convert.ToInt32(Row["x"]), Convert.ToInt32(Row["y"]), Convert.ToDouble(Row["z"]), Convert.ToInt32(Row["rot"]), Convert.ToInt32(Row["user_id"]),
                                Convert.ToInt32(Row["group_id"]), Convert.ToInt32(Row["limited_number"]), Convert.ToInt32(Row["limited_stack"]), Convert.ToString(Row["wall_pos"])));
                        }
                    }
                }
            }
            return I;
        }
    }
}
