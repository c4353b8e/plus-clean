namespace Plus.Communication.Packets.Incoming.Marketplace
{
    using System;
    using System.Data;
    using Outgoing.Marketplace;

    internal class GetMarketplaceItemStatsEvent : IPacketEvent
    {
        public void Parse(HabboHotel.GameClients.GameClient session, ClientPacket packet)
        {
            var itemId = packet.PopInt();
            var spriteId = packet.PopInt();

            DataRow row;
            using (var dbClient = Program.DatabaseManager.GetQueryReactor())
            {
                dbClient.SetQuery("SELECT `avgprice` FROM `catalog_marketplace_data` WHERE `sprite` = @SpriteId LIMIT 1");
                dbClient.AddParameter("SpriteId", spriteId);
                row = dbClient.GetRow();
            }

            session.SendPacket(new MarketplaceItemStatsComposer(itemId, spriteId, row != null ? Convert.ToInt32(row["avgprice"]) : 0));
        }
    }
}
