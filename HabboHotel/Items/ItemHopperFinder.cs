
namespace Plus.HabboHotel.Items
{
    using System;

    public static class ItemHopperFinder
    {
        public static int GetAHopper(int CurRoom)
        {
            using (var dbClient = Program.DatabaseManager.GetQueryReactor())
            {
                var RoomId = 0;
                dbClient.SetQuery("SELECT room_id FROM items_hopper WHERE room_id <> @room ORDER BY room_id ASC LIMIT 1");
                dbClient.AddParameter("room", CurRoom);
                RoomId = dbClient.GetInteger();
                return RoomId;
            }
        }

        public static int GetHopperId(int NextRoom)
        {
            using (var dbClient = Program.DatabaseManager.GetQueryReactor())
            {
                dbClient.SetQuery("SELECT hopper_id FROM items_hopper WHERE room_id = @room LIMIT 1");
                dbClient.AddParameter("room", NextRoom);
                var Row = dbClient.GetString();

                if (Row == null)
                {
                    return 0;
                }

                return Convert.ToInt32(Row);
            }
        }
    }
}