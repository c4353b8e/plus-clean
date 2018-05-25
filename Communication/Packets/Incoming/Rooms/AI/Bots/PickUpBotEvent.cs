namespace Plus.Communication.Packets.Incoming.Rooms.AI.Bots
{
    using System;
    using Game.Players;
    using Game.Users.Inventory.Bots;
    using Outgoing.Inventory.Bots;

    internal class PickUpBotEvent : IPacketEvent
    {
        public void Parse(Player session, ClientPacket packet)
        {
            if (!session.GetHabbo().InRoom)
            {
                return;
            }

            var botId = packet.PopInt();
            if (botId == 0)
            {
                return;
            }

            var room = session.GetHabbo().CurrentRoom;
            if (room == null)
            {
                return;
            }

            if (!room.GetRoomUserManager().TryGetBot(botId, out var botUser))
            {
                return;
            }

            if (session.GetHabbo().Id != botUser.BotData.OwnerId && !session.GetHabbo().GetPermissions().HasRight("bot_place_any_override"))
            {
                session.SendWhisper("You can only pick up your own bots!");
                return;
            }

            using (var dbClient = Program.DatabaseManager.GetQueryReactor())
            {
                dbClient.SetQuery("UPDATE `bots` SET `room_id` = '0' WHERE `id` = @id LIMIT 1");
                dbClient.AddParameter("id", botId);
                dbClient.RunQuery();
            }


            room.GetGameMap().RemoveUserFromMap(botUser, new System.Drawing.Point(botUser.X, botUser.Y));

            session.GetHabbo().GetInventoryComponent().TryAddBot(new Bot(Convert.ToInt32(botUser.BotData.Id), Convert.ToInt32(botUser.BotData.OwnerId), botUser.BotData.Name, botUser.BotData.Motto, botUser.BotData.Look, botUser.BotData.Gender));
            session.SendPacket(new BotInventoryComposer(session.GetHabbo().GetInventoryComponent().GetBots()));
            room.GetRoomUserManager().RemoveBot(botUser.VirtualId, false);
        }
    }
}
