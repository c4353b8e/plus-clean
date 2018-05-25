namespace Plus.Communication.Packets.Incoming.Catalog
{
    using HabboHotel.GameClients;
    using HabboHotel.Rooms;
    using Outgoing.Catalog;

    internal class GetCatalogRoomPromotionEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            var rooms = RoomFactory.GetRoomsDataByOwnerSortByName(session.GetHabbo().Id);

            session.SendPacket(new GetCatalogRoomPromotionComposer(rooms));
        }
    }
}
