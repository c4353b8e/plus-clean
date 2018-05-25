namespace Plus.Communication.Packets.Outgoing.Catalog
{
    using System.Collections.Generic;
    using HabboHotel.Rooms;

    internal class GetCatalogRoomPromotionComposer : ServerPacket
    {
        public GetCatalogRoomPromotionComposer(List<RoomData> UsersRooms)
            : base(ServerPacketHeader.PromotableRoomsMessageComposer)
        {
            WriteBoolean(true);//wat
            WriteInteger(UsersRooms.Count);//Count of rooms?
            foreach (var Room in UsersRooms)
            {
                WriteInteger(Room.Id);
               WriteString(Room.Name);
                WriteBoolean(true);
            }
        }
    }
}
