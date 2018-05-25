namespace Plus.Communication.Packets.Incoming.Catalog
{
    using System.Linq;
    using Game.Players;
    using Game.Rooms;
    using Outgoing.Catalog;
    using Utilities;

    internal class GetPromotableRoomsEvent : IPacketEvent
    {
        public void Parse(Player session, ClientPacket packet)
        {
            var rooms = RoomFactory.GetRoomsDataByOwnerSortByName(session.GetHabbo().Id);

            rooms = rooms.Where(x => x.Promotion == null || x.Promotion.TimestampExpires < UnixUtilities.GetNow()).ToList();

            session.SendPacket(new PromotableRoomsComposer(rooms));
        }
    }
}
