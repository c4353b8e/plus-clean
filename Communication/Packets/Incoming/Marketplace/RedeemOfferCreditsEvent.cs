namespace Plus.Communication.Packets.Incoming.Marketplace
{
    using System;
    using System.Data;
    using HabboHotel.GameClients;
    using Outgoing.Inventory.Purse;

    internal class RedeemOfferCreditsEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            var creditsOwed = 0;

            DataTable table;
            using (var dbClient = Program.DatabaseManager.GetQueryReactor())
            {
                dbClient.SetQuery("SELECT `asking_price` FROM `catalog_marketplace_offers` WHERE `user_id` = '" + session.GetHabbo().Id + "' AND `state` = '2'");
               table = dbClient.GetTable();
            }

            if (table != null)
            {
                foreach (DataRow row in table.Rows)
                {
                    creditsOwed += Convert.ToInt32(row["asking_price"]);
                }

                if (creditsOwed >= 1)
                {
                    session.GetHabbo().Credits += creditsOwed;
                    session.SendPacket(new CreditBalanceComposer(session.GetHabbo().Credits));
                }

                using (var dbClient = Program.DatabaseManager.GetQueryReactor())
                {
                    dbClient.RunQuery("DELETE FROM `catalog_marketplace_offers` WHERE `user_id` = '" + session.GetHabbo().Id + "' AND `state` = '2'");
                }
            }
        }
    }
}