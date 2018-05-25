namespace Plus.Communication.Packets.Incoming.Catalog
{
    using HabboHotel.GameClients;
    using Outgoing.Catalog;

    internal class GetCatalogOfferEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            var offerId = packet.PopInt();
            if (!Program.GameContext.GetCatalog().ItemOffers.ContainsKey(offerId))
            {
                return;
            }

            var pageId = Program.GameContext.GetCatalog().ItemOffers[offerId];

            if (!Program.GameContext.GetCatalog().TryGetPage(pageId, out var page))
            {
                return;
            }

            if (!page.Enabled || !page.Visible || page.MinimumRank > session.GetHabbo().Rank || page.MinimumVIP > session.GetHabbo().VIPRank && session.GetHabbo().Rank == 1)
            {
                return;
            }

            if (!page.ItemOffers.ContainsKey(offerId))
            {
                return;
            }

            var item = page.ItemOffers[offerId];
            if (item != null)
            {
                session.SendPacket(new CatalogOfferComposer(item));
            }
        }
    }
}
