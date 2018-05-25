namespace Plus.Communication.Packets.Incoming.Catalog
{
    using HabboHotel.GameClients;
    using Outgoing.BuildersClub;
    using Outgoing.Catalog;

    public class GetCatalogIndexEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            session.SendPacket(new CatalogIndexComposer(session, Program.GameContext.GetCatalog().GetPages()));
            session.SendPacket(new CatalogItemDiscountComposer());
            session.SendPacket(new BCBorrowedItemsComposer());
        }
    }
}