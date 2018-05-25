namespace Plus.Communication.Packets.Incoming.Catalog
{
    using Game.Players;
    using Game.Rooms;
    using Outgoing.Catalog;

    internal class GetCatalogRoomPromotionEvent : IPacketEvent
    {
        public void Parse(Player session, ClientPacket packet)
        {
            var rooms = RoomFactory.GetRoomsDataByOwnerSortByName(session.GetHabbo().Id);

            session.SendPacket(new GetCatalogRoomPromotionComposer(rooms));
        }
    }
}
