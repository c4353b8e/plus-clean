namespace Plus.Communication.Packets.Incoming.Marketplace
{
    using System;
    using System.Data;
    using HabboHotel.GameClients;
    using HabboHotel.Items;
    using Outgoing.Inventory.Furni;
    using Outgoing.Marketplace;

    internal class CancelOfferEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            if (session?.GetHabbo() == null)
            {
                return;
            }

            DataRow row;
            var offerId = packet.PopInt();

            using (var dbClient = Program.DatabaseManager.GetQueryReactor())
            {
                dbClient.SetQuery("SELECT `furni_id`, `item_id`, `user_id`, `extra_data`, `offer_id`, `state`, `timestamp`, `limited_number`, `limited_stack` FROM `catalog_marketplace_offers` WHERE `offer_id` = @OfferId LIMIT 1");
                dbClient.AddParameter("OfferId", offerId);
                row = dbClient.GetRow();
            }

            if (row == null)
            {
                session.SendPacket(new MarketplaceCancelOfferResultComposer(offerId, false));
                return;
            }

            if (Convert.ToInt32(row["user_id"]) != session.GetHabbo().Id)
            {
                session.SendPacket(new MarketplaceCancelOfferResultComposer(offerId, false));
                return;
            }

            if (!Program.GameContext.GetItemManager().GetItem(Convert.ToInt32(row["item_id"]), out var item))
            {
                session.SendPacket(new MarketplaceCancelOfferResultComposer(offerId, false));
                return;
            }
            
            var giveItem = ItemFactory.CreateSingleItem(item, session.GetHabbo(), Convert.ToString(row["extra_data"]), Convert.ToString(row["extra_data"]), Convert.ToInt32(row["furni_id"]), Convert.ToInt32(row["limited_number"]), Convert.ToInt32(row["limited_stack"]));
            session.SendPacket(new FurniListNotificationComposer(giveItem.Id, 1));
            session.SendPacket(new FurniListUpdateComposer());

            using (var dbClient = Program.DatabaseManager.GetQueryReactor())
            {
                dbClient.SetQuery("DELETE FROM `catalog_marketplace_offers` WHERE `offer_id` = @OfferId AND `user_id` = @UserId LIMIT 1");
                dbClient.AddParameter("OfferId", offerId);
                dbClient.AddParameter("UserId", session.GetHabbo().Id);
                dbClient.RunQuery();
            }

            session.GetHabbo().GetInventoryComponent().UpdateItems(true);
            session.SendPacket(new MarketplaceCancelOfferResultComposer(offerId, true));
        }
    }
}
