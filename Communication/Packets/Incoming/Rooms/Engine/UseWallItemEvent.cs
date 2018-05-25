namespace Plus.Communication.Packets.Incoming.Rooms.Engine
{
    using HabboHotel.GameClients;
    using HabboHotel.Items.Wired;
    using HabboHotel.Quests;

    internal class UseWallItemEvent : IPacketEvent
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

            var request = packet.PopInt();

            item.Interactor.OnTrigger(session, item, request, hasRights);
            item.GetRoom().GetWired().TriggerEvent(WiredBoxType.TriggerStateChanges, session.GetHabbo(), item);
        
            Program.GameContext.GetQuestManager().ProgressUserQuest(session, QuestType.ExploreFindItem, item.GetBaseItem().Id);
        }
    }
}
