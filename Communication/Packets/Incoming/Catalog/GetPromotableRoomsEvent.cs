namespace Plus.Communication.Packets.Incoming.Catalog
{
    using System.Linq;
    using HabboHotel.GameClients;
    using HabboHotel.Rooms;
    using Outgoing.Catalog;
    using Utilities;

    internal class GetPromotableRoomsEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            var rooms = RoomFactory.GetRoomsDataByOwnerSortByName(session.GetHabbo().Id);

            rooms = rooms.Where(x => x.Promotion == null || x.Promotion.TimestampExpires < UnixTimestamp.GetNow()).ToList();

            session.SendPacket(new PromotableRoomsComposer(rooms));
        }
    }
}
