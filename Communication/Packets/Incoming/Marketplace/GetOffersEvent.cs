namespace Plus.Communication.Packets.Incoming.Marketplace
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Text;
    using HabboHotel.Catalog.Marketplace;
    using HabboHotel.GameClients;
    using Outgoing.Marketplace;

    internal class GetOffersEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            var minCost = packet.PopInt();
            var maxCost = packet.PopInt();
            var searchQuery = packet.PopString();
            var filterMode = packet.PopInt();


            DataTable table;
            var builder = new StringBuilder();
            string str;
            builder.Append("WHERE `state` = '1' AND `timestamp` >= " + Program.GameContext.GetCatalog().GetMarketplace().FormatTimestampString());
            if (minCost >= 0)
            {
                builder.Append(" AND `total_price` > " + minCost);
            }


            if (maxCost >= 0)
            {
                builder.Append(" AND `total_price` < " + maxCost);
            }

            switch (filterMode)
            {
                case 1:
                    str = "ORDER BY `asking_price` DESC";
                    break;

                default:
                    str = "ORDER BY `asking_price` ASC";
                    break;
            }

            using (var dbClient = Program.DatabaseManager.GetQueryReactor())
            {

                dbClient.SetQuery("SELECT `offer_id`, `item_type`, `sprite_id`, `total_price`, `limited_number`,`limited_stack` FROM `catalog_marketplace_offers` " + builder + " " + str + " LIMIT 500");
                dbClient.AddParameter("search_query", "%" + searchQuery + "%");
                if (searchQuery.Length >= 1)
                {
                    builder.Append(" AND `public_name` LIKE @search_query");
                }
                table = dbClient.GetTable();
            }

            Program.GameContext.GetCatalog().GetMarketplace().MarketItems.Clear();
            Program.GameContext.GetCatalog().GetMarketplace().MarketItemKeys.Clear();
            if (table != null)
            {
                foreach (DataRow row in table.Rows)
                {
                    if (!Program.GameContext.GetCatalog().GetMarketplace().MarketItemKeys.Contains(Convert.ToInt32(row["offer_id"])))
                    {
                        Program.GameContext.GetCatalog().GetMarketplace().MarketItemKeys.Add(Convert.ToInt32(row["offer_id"]));
                        Program.GameContext.GetCatalog().GetMarketplace().MarketItems.Add(new MarketOffer(Convert.ToInt32(row["offer_id"]), Convert.ToInt32(row["sprite_id"]), Convert.ToInt32(row["total_price"]), int.Parse(row["item_type"].ToString()), Convert.ToInt32(row["limited_number"]), Convert.ToInt32(row["limited_stack"])));
                    }
                }
            }

            var dictionary = new Dictionary<int, MarketOffer>();
            var dictionary2 = new Dictionary<int, int>();

            foreach (var item in Program.GameContext.GetCatalog().GetMarketplace().MarketItems)
            {
                if (dictionary.ContainsKey(item.SpriteId))
                {
                    if (item.LimitedNumber > 0)
                    {
                        if (!dictionary.ContainsKey(item.OfferID))
                        {
                            dictionary.Add(item.OfferID, item);
                        }

                        if (!dictionary2.ContainsKey(item.OfferID))
                        {
                            dictionary2.Add(item.OfferID, 1);
                        }
                    }
                    else
                    {
                        if (dictionary[item.SpriteId].TotalPrice > item.TotalPrice)
                        {
                            dictionary.Remove(item.SpriteId);
                            dictionary.Add(item.SpriteId, item);
                        }

                        var num = dictionary2[item.SpriteId];
                        dictionary2.Remove(item.SpriteId);
                        dictionary2.Add(item.SpriteId, num + 1);
                    }
                }
                else
                {
                    if (!dictionary.ContainsKey(item.SpriteId))
                    {
                        dictionary.Add(item.SpriteId, item);
                    }

                    if (!dictionary2.ContainsKey(item.SpriteId))
                    {
                        dictionary2.Add(item.SpriteId, 1);
                    }
                }
            }

            session.SendPacket(new MarketPlaceOffersComposer(dictionary, dictionary2));
        }
    }
}
