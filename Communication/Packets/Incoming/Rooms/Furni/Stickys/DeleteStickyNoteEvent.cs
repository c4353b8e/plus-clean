namespace Plus.Communication.Packets.Incoming.Rooms.Furni.Stickys
{
    using HabboHotel.GameClients;
    using HabboHotel.Items;

    internal class DeleteStickyNoteEvent : IPacketEvent
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

            if (!room.CheckRights(session))
            {
                return;
            }

            var item = room.GetRoomItemHandler().GetItem(packet.PopInt());
            if (item == null)
            {
                return;
            }

            if (item.GetBaseItem().InteractionType == InteractionType.POSTIT || item.GetBaseItem().InteractionType == InteractionType.CAMERA_PICTURE)
            {
                room.GetRoomItemHandler().RemoveFurniture(session, item.Id);
                using (var dbClient = Program.DatabaseManager.GetQueryReactor())
                {
                    dbClient.RunQuery("DELETE FROM `items` WHERE `id` = '" + item.Id + "' LIMIT 1");
                }
            }
        }
    }
}
