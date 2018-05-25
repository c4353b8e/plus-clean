namespace Plus.Communication.Packets.Incoming.Rooms.Furni
{
    using Game.Items;
    using Game.Players;
    using Outgoing.Rooms.Engine;

    internal class SetTonerEvent : IPacketEvent
    {
        public void Parse(Player session, ClientPacket packet)
        {
            if (!Program.GameContext.GetRoomManager().TryGetRoom(session.GetHabbo().CurrentRoomId, out var room))
            {
                return;
            }

            if (!room.CheckRights(session, true))
            {
                return;
            }

            if (room.TonerData == null)
            {
                return;
            }

            var item = room.GetRoomItemHandler().GetItem(room.TonerData.ItemId);

            if (item == null || item.GetBaseItem().InteractionType != InteractionType.TONER)
            {
                return;
            }

            packet.PopInt(); //id
            var int1 = packet.PopInt();
            var int2 = packet.PopInt();
            var int3 = packet.PopInt();

            if (int1 > 255 || int2 > 255 || int3 > 255)
            {
                return;
            }

            using (var dbClient = Program.DatabaseManager.GetQueryReactor())
            {
                dbClient.SetQuery("UPDATE `room_items_toner` SET `enabled` = '1', `data1` = @data1, `data2` = @data2, `data3` = @data3 WHERE `id` = @itemId LIMIT 1");
                dbClient.AddParameter("itemId", item.Id);
                dbClient.AddParameter("data1", int1);
                dbClient.AddParameter("data3", int3);
                dbClient.AddParameter("data2", int2);
                dbClient.RunQuery();
            }

            room.TonerData.Hue = int1;
            room.TonerData.Saturation = int2;
            room.TonerData.Lightness = int3;
            room.TonerData.Enabled = 1;

            room.SendPacket(new ObjectUpdateComposer(item, room.OwnerId));
            item.UpdateState();
        }
    }
}
