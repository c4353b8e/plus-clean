namespace Plus.Communication.Packets.Incoming.Rooms.AI.Bots
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using Game.Players;
    using Game.Rooms.AI;
    using Game.Rooms.AI.Speech;
    using Outgoing.Inventory.Bots;

    internal class PlaceBotEvent : IPacketEvent
    {
        public void Parse(Player session, ClientPacket packet)
        {
            if (!session.GetHabbo().InRoom)
            {
                return;
            }

            if (!Program.GameContext.GetRoomManager().TryGetRoom(session.GetHabbo().CurrentRoomId, out var room))
            {
                return;
            }

            if (!room.CheckRights(session, true))
            {
                return;
            }

            var botId = packet.PopInt();
            var x = packet.PopInt();
            var y = packet.PopInt();

            if (!room.GetGameMap().CanWalk(x, y, false) || !room.GetGameMap().ValidTile(x, y))
            {
                session.SendNotification("You cannot place a bot here!");
                return;
            }
            
            if (!session.GetHabbo().GetInventoryComponent().TryGetBot(botId, out var bot))
            {
                return;
            }

            var botCount = 0;
            foreach (var user in room.GetRoomUserManager().GetUserList().ToList())
            {
                if (user == null || user.IsPet || !user.IsBot)
                {
                    continue;
                }

                botCount += 1;
            }

            if (botCount >= 5 && !session.GetHabbo().GetPermissions().HasRight("bot_place_any_override"))
            {
                session.SendNotification("Sorry; 5 bots per room only!");
                return;
            }

            //TODO: Hmm, maybe not????
            using (var dbClient = Program.DatabaseManager.GetQueryReactor())
            {
                dbClient.SetQuery("UPDATE `bots` SET `room_id` = @roomId, `x` = @CoordX, `y` = @CoordY WHERE `id` = @BotId LIMIT 1");
                dbClient.AddParameter("roomId", room.RoomId);
                dbClient.AddParameter("BotId", bot.Id);
                dbClient.AddParameter("CoordX", x);
                dbClient.AddParameter("CoordY", y);
                dbClient.RunQuery();
            }

            var botSpeechList = new List<RandomSpeech>();

            //TODO: Grab data?
            DataRow getData;
            using (var dbClient = Program.DatabaseManager.GetQueryReactor())
            {
                dbClient.SetQuery("SELECT `ai_type`,`rotation`,`walk_mode`,`automatic_chat`,`speaking_interval`,`mix_sentences`,`chat_bubble` FROM `bots` WHERE `id` = @BotId LIMIT 1");
                dbClient.AddParameter("BotId", bot.Id);
                getData = dbClient.GetRow();

                dbClient.SetQuery("SELECT `text` FROM `bots_speech` WHERE `bot_id` = @BotId");
                dbClient.AddParameter("BotId", bot.Id);
                var botSpeech = dbClient.GetTable();

                foreach (DataRow speech in botSpeech.Rows)
                {
                    botSpeechList.Add(new RandomSpeech(Convert.ToString(speech["text"]), bot.Id));
                }
            }

            var botUser = room.GetRoomUserManager().DeployBot(new RoomBot(bot.Id, session.GetHabbo().CurrentRoomId, Convert.ToString(getData["ai_type"]), Convert.ToString(getData["walk_mode"]), bot.Name, "", bot.Figure, x, y, 0, 4, 0, 0, 0, 0, ref botSpeechList, "", 0, bot.OwnerId, getData["automatic_chat"].ToString() == "1", Convert.ToInt32(getData["speaking_interval"]), getData["mix_sentences"].ToString() == "1", Convert.ToInt32(getData["chat_bubble"])), null);
            botUser.Chat("Hello!");

            room.GetGameMap().UpdateUserMovement(new System.Drawing.Point(x,y), new System.Drawing.Point(x, y), botUser);


            if (!session.GetHabbo().GetInventoryComponent().TryRemoveBot(botId, out var toRemove))
            {
                Console.WriteLine("Error whilst removing Bot: " + toRemove.Id);
                return;
            }
            session.SendPacket(new BotInventoryComposer(session.GetHabbo().GetInventoryComponent().GetBots()));
        }
    }
}
