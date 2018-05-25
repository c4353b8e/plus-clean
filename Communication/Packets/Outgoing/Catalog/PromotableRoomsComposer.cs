namespace Plus.Communication.Packets.Outgoing.Catalog
{
    using System.Collections.Generic;
    using HabboHotel.Rooms;

    internal class PromotableRoomsComposer : ServerPacket
    {
        public PromotableRoomsComposer(ICollection<RoomData> Rooms)
            : base(ServerPacketHeader.PromotableRoomsMessageComposer)
        {
            WriteBoolean(true);
            WriteInteger(Rooms.Count);//Count

            foreach (var Data in Rooms)
            {
                WriteInteger(Data.Id);
               WriteString(Data.Name);
                WriteBoolean(false);
            }
        }
    }
}