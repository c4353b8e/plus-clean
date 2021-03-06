﻿namespace Plus.Game.Catalog.Utilities
{
    using System;
    using System.Data;
    using Items;
    using Rooms.AI;
    using Users.Inventory.Bots;

    public static class BotUtility
    {
        public static Bot CreateBot(ItemData itemData, int ownerId)
        {
            DataRow bot = null;
            if (!Program.GameContext.GetCatalog().TryGetBot(itemData.Id, out var CataBot))
            {
                return null;
            }

            using (var dbClient = Program.DatabaseManager.GetQueryReactor())
            {
                dbClient.SetQuery("INSERT INTO bots (`user_id`,`name`,`motto`,`look`,`gender`,`ai_type`) VALUES ('" + ownerId + "', '" + CataBot.Name + "', '" + CataBot.Motto + "', '" + CataBot.Figure + "', '" + CataBot.Gender + "', '" + CataBot.AIType + "')");
                var id = Convert.ToInt32(dbClient.InsertQuery());

                dbClient.SetQuery("SELECT `id`,`user_id`,`name`,`motto`,`look`,`gender` FROM `bots` WHERE `user_id` = '" + ownerId + "' AND `id` = '" + id + "' LIMIT 1");
                bot = dbClient.GetRow();
            }

            return new Bot(Convert.ToInt32(bot["id"]), Convert.ToInt32(bot["user_id"]), Convert.ToString(bot["name"]), Convert.ToString(bot["motto"]), Convert.ToString(bot["look"]), Convert.ToString(bot["gender"]));
        }


        public static BotAIType GetAIFromString(string type)
        {
            switch (type)
            {
                case "pet":
                    return BotAIType.Pet;
                case "generic":
                    return BotAIType.Generic;
                case "bartender":
                    return BotAIType.Bartender;
                case "casino_bot":
                    return BotAIType.CasinoBot;
                default:
                    return BotAIType.Generic;
            }
        }
    }
}