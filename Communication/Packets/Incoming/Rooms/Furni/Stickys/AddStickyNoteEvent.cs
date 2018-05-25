namespace Plus.Communication.Packets.Incoming.Rooms.Furni.Stickys
{
    using System;
    using System.Linq;
    using Game.Items;
    using Game.Players;

    internal class AddStickyNoteEvent : IPacketEvent
    {
        public void Parse(Player session, ClientPacket packet)
        {
            var itemId = packet.PopInt();
            var locationData = packet.PopString();

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

            var item = session.GetHabbo().GetInventoryComponent().GetItem(itemId);
            if (item == null)
            {
                return;
            }

            try
            {
                var wallPossition = WallPositionCheck(":" + locationData.Split(':')[1]);

                var roomItem = new Item(item.Id, room.RoomId, item.BaseItem, item.ExtraData, 0, 0, 0, 0, session.GetHabbo().Id, item.GroupId, 0, 0, wallPossition, room);
                if (room.GetRoomItemHandler().SetWallItem(session, roomItem))
                {
                    session.GetHabbo().GetInventoryComponent().RemoveItem(itemId);
                }
            }
            catch
            {
                //TODO: Send a packet
            }
        }

        private static string WallPositionCheck(string wallPosition)
        {
            //:w=3,2 l=9,63 l
            try
            {
                if (wallPosition.Contains(Convert.ToChar(13)))
                {
                    return null;
                }
                if (wallPosition.Contains(Convert.ToChar(9)))
                {
                    return null;
                }

                var posD = wallPosition.Split(' ');
                if (posD[2] != "l" && posD[2] != "r")
                {
                    return null;
                }

                var widD = posD[0].Substring(3).Split(',');
                var widthX = int.Parse(widD[0]);
                var widthY = int.Parse(widD[1]);
                if (widthX < 0 || widthY < 0 || widthX > 200 || widthY > 200)
                {
                    return null;
                }

                var lenD = posD[1].Substring(2).Split(',');
                var lengthX = int.Parse(lenD[0]);
                var lengthY = int.Parse(lenD[1]);
                if (lengthX < 0 || lengthY < 0 || lengthX > 200 || lengthY > 200)
                {
                    return null;
                }

                return ":w=" + widthX + "," + widthY + " " + "l=" + lengthX + "," + lengthY + " " + posD[2];
            }
            catch
            {
                return null;
            }
        }
    }
}
