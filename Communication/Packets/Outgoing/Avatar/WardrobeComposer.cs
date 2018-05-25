namespace Plus.Communication.Packets.Outgoing.Avatar
{
    using System;
    using System.Data;

    internal class WardrobeComposer : ServerPacket
    {
        public WardrobeComposer(int userId)
            : base(ServerPacketHeader.WardrobeMessageComposer)
        {
            WriteInteger(1);
            using (var dbClient = Program.DatabaseManager.GetQueryReactor())
            {
                dbClient.SetQuery("SELECT `slot_id`,`look`,`gender` FROM `user_wardrobe` WHERE `user_id` = '" + userId + "'");
                var WardrobeData = dbClient.GetTable();

                if (WardrobeData == null)
                {
                    WriteInteger(0);
                }
                else
                {
                    WriteInteger(WardrobeData.Rows.Count);
                    foreach (DataRow Row in WardrobeData.Rows)
                    {
                        WriteInteger(Convert.ToInt32(Row["slot_id"]));
                        WriteString(Convert.ToString(Row["look"]));
                        WriteString(Row["gender"].ToString().ToUpper());
                    }
                }
            }
        }
    }
}