namespace Plus.Communication.Packets.Incoming.Rooms.Engine
{
    using HabboHotel.GameClients;
    using HabboHotel.Items;
    using HabboHotel.Quests;
    using Outgoing.Rooms.Engine;

    internal class ApplyDecorationEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
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

            var item = session.GetHabbo().GetInventoryComponent().GetItem(packet.PopInt());
            if (item == null)
            {
                return;
            }

            if (item.GetBaseItem() == null)
            {
                return;
            }

            var decorationKey = string.Empty;
            switch (item.GetBaseItem().InteractionType)
            {
                case InteractionType.FLOOR:
                    decorationKey = "floor";
                    break;

                case InteractionType.WALLPAPER:
                    decorationKey = "wallpaper";
                    break;

                case InteractionType.LANDSCAPE:
                    decorationKey = "landscape";
                    break;
            }

            switch (decorationKey)
            {
                case "floor":
                    room.Floor = item.ExtraData;

                    Program.GameContext.GetQuestManager().ProgressUserQuest(session, QuestType.FurniDecoFloor);
                    Program.GameContext.GetAchievementManager().ProgressAchievement(session, "ACH_RoomDecoFloor", 1);
                    break;

                case "wallpaper":
                    room.Wallpaper = item.ExtraData;

                    Program.GameContext.GetQuestManager().ProgressUserQuest(session, QuestType.FurniDecoWall);
                    Program.GameContext.GetAchievementManager().ProgressAchievement(session, "ACH_RoomDecoWallpaper", 1);
                    break;

                case "landscape":
                    room.Landscape = item.ExtraData;

                    Program.GameContext.GetAchievementManager().ProgressAchievement(session, "ACH_RoomDecoLandscape", 1);
                    break;
            }

            using (var dbClient = Program.DatabaseManager.GetQueryReactor())
            {
                dbClient.SetQuery("UPDATE `rooms` SET `" + decorationKey + "` = @extradata WHERE `id` = '" + room.RoomId + "' LIMIT 1");
                dbClient.AddParameter("extradata", item.ExtraData);
                dbClient.RunQuery();

                dbClient.RunQuery("DELETE FROM `items` WHERE `id` = '" + item.Id + "' LIMIT 1");
            }

            session.GetHabbo().GetInventoryComponent().RemoveItem(item.Id);
            room.SendPacket(new RoomPropertyComposer(decorationKey, item.ExtraData));
        }
    }
}
