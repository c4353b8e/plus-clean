namespace Plus.Game.Items
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using Core.Logging;
    using Televisions;

    public class ItemDataManager
    {
        private static readonly ILogger Logger = new Logger<ItemDataManager>();

        public Dictionary<int, ItemData> _items;
        public Dictionary<int, ItemData> _gifts;//<SpriteId, Item>

        private readonly TelevisionManager _televisionManager;

        public ItemDataManager()
        {
            _televisionManager = new TelevisionManager();
            _televisionManager.Init();

            _items = new Dictionary<int, ItemData>();
            _gifts = new Dictionary<int, ItemData>();

            Init();
        }

        public TelevisionManager GetTelevisionManager()
        {
            return _televisionManager;
        }

        public void Init()
        {
            if (_items.Count > 0)
            {
                _items.Clear();
            }

            using (var dbClient = Program.DatabaseManager.GetQueryReactor())
            {
                dbClient.SetQuery("SELECT * FROM `furniture`");
                var ItemData = dbClient.GetTable();

                if (ItemData != null)
                {
                    foreach (DataRow Row in ItemData.Rows)
                    {
                        try
                        {
                            var id = Convert.ToInt32(Row["id"]);
                            var spriteID = Convert.ToInt32(Row["sprite_id"]);
                            var itemName = Convert.ToString(Row["item_name"]);
                            var PublicName = Convert.ToString(Row["public_name"]);
                            var type = Row["type"].ToString();
                            var width = Convert.ToInt32(Row["width"]);
                            var length = Convert.ToInt32(Row["length"]);
                            var height = Convert.ToDouble(Row["stack_height"]);
                            var allowStack = Row["can_stack"].ToString() == "1";
                            var allowWalk = Row["is_walkable"].ToString() == "1";
                            var allowSit = Row["can_sit"].ToString() == "1";
                            var allowRecycle = Row["allow_recycle"].ToString() == "1";
                            var allowTrade = Row["allow_trade"].ToString() == "1";
                            var allowMarketplace = Convert.ToInt32(Row["allow_marketplace_sell"]) == 1;
                            var allowGift = Convert.ToInt32(Row["allow_gift"]) == 1;
                            var allowInventoryStack = Row["allow_inventory_stack"].ToString() == "1";
                            var interactionType = InteractionTypes.GetTypeFromString(Convert.ToString(Row["interaction_type"]));
                            var behaviourData = Convert.ToInt32(Row["behaviour_data"]);
                            var cycleCount = Convert.ToInt32(Row["interaction_modes_count"]);
                            var vendingIDS = Convert.ToString(Row["vending_ids"]);
                            var heightAdjustable = Convert.ToString(Row["height_adjustable"]);
                            var EffectId = Convert.ToInt32(Row["effect_id"]);
                            var IsRare = Row["is_rare"].ToString() == "1";
                            var ExtraRot = Row["extra_rot"].ToString() == "1";

                            if (!_gifts.ContainsKey(spriteID))
                            {
                                _gifts.Add(spriteID, new ItemData(id, spriteID, itemName, PublicName, type, width, length, height, allowStack, allowWalk, allowSit, allowRecycle, allowTrade, allowMarketplace, allowGift, allowInventoryStack, interactionType, behaviourData, cycleCount, vendingIDS, heightAdjustable, EffectId, IsRare, ExtraRot));
                            }

                            if (!_items.ContainsKey(id))
                            {
                                _items.Add(id, new ItemData(id, spriteID, itemName, PublicName, type, width, length, height, allowStack, allowWalk, allowSit, allowRecycle, allowTrade, allowMarketplace, allowGift, allowInventoryStack, interactionType, behaviourData, cycleCount, vendingIDS, heightAdjustable, EffectId, IsRare, ExtraRot));
                            }
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e.ToString());
                            Console.ReadKey();
                        }
                    }
                }
            }

            Logger.Trace("Item Manager -> LOADED");
        }

        public bool GetItem(int Id, out ItemData Item)
        {
            if (_items.TryGetValue(Id, out Item))
            {
                return true;
            }

            return false;
        }

        public ItemData GetItemByName(string name)
        {
            foreach (var entry in _items)
            {
                var item = entry.Value;
                if (item.ItemName == name)
                {
                    return item;
                }
            }
            return null;
        }

        public bool GetGift(int SpriteId, out ItemData Item)
        {
            if (_gifts.TryGetValue(SpriteId, out Item))
            {
                return true;
            }

            return false;
        }
    }
}