namespace Plus.HabboHotel.Items.Data.Toner
{
    using System;
    using System.Data;

    public class TonerData
    {
        public int ItemId;
        public int Hue;
        public int Saturation;
        public int Lightness;
        public int Enabled;

        public TonerData(int Item)
        {
            ItemId = Item;

            DataRow Row;

            using (var dbClient = Program.DatabaseManager.GetQueryReactor())
            {
                dbClient.SetQuery("SELECT enabled,data1,data2,data3 FROM room_items_toner WHERE id=" + ItemId +" LIMIT 1");
                Row = dbClient.GetRow();
            }

            if (Row == null)
            {
                //throw new NullReferenceException("No toner data found in the database for " + ItemId);
                using (var dbClient = Program.DatabaseManager.GetQueryReactor())
                {
                    dbClient.RunQuery("INSERT INTO `room_items_toner` VALUES (" + ItemId + ",'0',0,0,0)");
                    dbClient.SetQuery("SELECT enabled,data1,data2,data3 FROM room_items_toner WHERE id=" + ItemId + " LIMIT 1");
                    Row = dbClient.GetRow();
                }
            }

            Enabled = int.Parse(Row[0].ToString());
            Hue = Convert.ToInt32(Row[1]);
            Saturation = Convert.ToInt32(Row[2]);
            Lightness = Convert.ToInt32(Row[3]);
        }
    }
}