namespace Plus.HabboHotel.Items
{
    using System;
    using Rooms;

    public static class ItemTeleporterFinder
    {
        public static int GetLinkedTele(int TeleId)
        {
            using (var dbClient = Program.DatabaseManager.GetQueryReactor())
            {
                dbClient.SetQuery("SELECT `tele_two_id` FROM `room_items_tele_links` WHERE `tele_one_id` = '" + TeleId + "' LIMIT 1");
                var Row = dbClient.GetRow();

                if (Row == null)
                {
                    return 0;
                }

                return Convert.ToInt32(Row[0]);
            }
        }

        public static int GetTeleRoomId(int TeleId, Room pRoom)
        {
            if (pRoom.GetRoomItemHandler().GetItem(TeleId) != null)
            {
                return pRoom.RoomId;
            }

            using (var dbClient = Program.DatabaseManager.GetQueryReactor())
            {
                dbClient.SetQuery("SELECT `room_id` FROM `items` WHERE `id` = " + TeleId + " LIMIT 1");
                var Row = dbClient.GetRow();

                if (Row == null)
                {
                    return 0;
                }

                return Convert.ToInt32(Row[0]);
            }
        }

        public static bool IsTeleLinked(int TeleId, Room pRoom)
        {
            var LinkId = GetLinkedTele(TeleId);

            if (LinkId == 0)
            {
                return false;
            }


            var item = pRoom.GetRoomItemHandler().GetItem(LinkId);
            if (item != null && item.GetBaseItem().InteractionType == InteractionType.TELEPORT)
            {
                return true;
            }

            var RoomId = GetTeleRoomId(LinkId, pRoom);

            if (RoomId == 0)
            {
                return false;
            }

            return true;
        }
    }
}