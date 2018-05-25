namespace Plus.Communication.Packets.Incoming.Rooms.Engine
{
    using HabboHotel.GameClients;
    using HabboHotel.Items;
    using HabboHotel.Items.Wired;
    using HabboHotel.Quests;
    using HabboHotel.Rooms;
    using Outgoing.Rooms.Engine;
    using Outgoing.Rooms.Furni;

    internal class UseFurnitureEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            if (session == null || session.GetHabbo() == null || !session.GetHabbo().InRoom)
            {
                return;
            }

            if (!Program.GameContext.GetRoomManager().TryGetRoom(session.GetHabbo().CurrentRoomId, out var room))
            {
                return;
            }

            var itemId = packet.PopInt();
            var item = room.GetRoomItemHandler().GetItem(itemId);
            if (item == null)
            {
                return;
            }

            var hasRights = room.CheckRights(session, false, true);

            if (item.GetBaseItem().InteractionType == InteractionType.banzaitele)
            {
                return;
            }

            if (item.GetBaseItem().InteractionType == InteractionType.TONER)
            {
                if (!room.CheckRights(session, true))
                {
                    return;
                }

                room.TonerData.Enabled = room.TonerData.Enabled == 0 ? 1 : 0;

                room.SendPacket(new ObjectUpdateComposer(item, room.OwnerId));

                item.UpdateState();

                using (var dbClient = Program.DatabaseManager.GetQueryReactor())
                {
                    dbClient.RunQuery("UPDATE `room_items_toner` SET `enabled` = '" + room.TonerData.Enabled + "' LIMIT 1");
                }
                return;
            }

            if (item.Data.InteractionType == InteractionType.GNOME_BOX && item.UserID == session.GetHabbo().Id)
            {
                session.SendPacket(new GnomeBoxComposer(item.Id));
            }

            var toggle = true;
            if (item.GetBaseItem().InteractionType == InteractionType.WF_FLOOR_SWITCH_1 || item.GetBaseItem().InteractionType == InteractionType.WF_FLOOR_SWITCH_2)
            {
                var user = item.GetRoom().GetRoomUserManager().GetRoomUserByHabbo(session.GetHabbo().Id);
                if (user == null)
                {
                    return;
                }

                if (!Gamemap.TilesTouching(item.GetX, item.GetY, user.X, user.Y))
                {
                    toggle = false;
                }
            }

            var request = packet.PopInt();

            item.Interactor.OnTrigger(session, item, request, hasRights);

            if (toggle)
            {
                item.GetRoom().GetWired().TriggerEvent(WiredBoxType.TriggerStateChanges, session.GetHabbo(), item);
            }

            Program.GameContext.GetQuestManager().ProgressUserQuest(session, QuestType.ExploreFindItem, item.GetBaseItem().Id);
      
        }
    }
}
