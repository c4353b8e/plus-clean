namespace Plus.Communication.Packets.Incoming.Catalog
{
    using HabboHotel.GameClients;
    using Outgoing.Catalog;

    public class GetCatalogPageEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            var pageId = packet.PopInt();
            packet.PopInt();
            var cataMode = packet.PopString();

            if (!Program.GameContext.GetCatalog().TryGetPage(pageId, out var page))
            {
                return;
            }

            if (!page.Enabled || !page.Visible || page.MinimumRank > session.GetHabbo().Rank || page.MinimumVIP > session.GetHabbo().VIPRank && session.GetHabbo().Rank == 1)
            {
                return;
            }

            session.SendPacket(new CatalogPageComposer(page, cataMode));
        }
    }
}