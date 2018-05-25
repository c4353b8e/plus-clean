namespace Plus.Communication.Packets.Incoming.Rooms.Engine
{
    using HabboHotel.GameClients;
    using Outgoing.Rooms.Engine;

    internal class MoveWallItemEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            if (!Program.GameContext.GetRoomManager().TryGetRoom(session.GetHabbo().CurrentRoomId, out var room))
            {
                return;
            }

            if (!room.CheckRights(session))
            {
                return;
            }

            var itemId = packet.PopInt();
            var wallPositionData = packet.PopString();

            var item = room.GetRoomItemHandler().GetItem(itemId);

            if (item == null)
            {
                return;
            }

            try
            {
                var wallPos = room.GetRoomItemHandler().WallPositionCheck(":" + wallPositionData.Split(':')[1]);
                item.wallCoord = wallPos;
            }
            catch { return; }

            room.GetRoomItemHandler().UpdateItem(item);
            room.SendPacket(new ItemUpdateComposer(item, room.OwnerId));
        }
    }
}
