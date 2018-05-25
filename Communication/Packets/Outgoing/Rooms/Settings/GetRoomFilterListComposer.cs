namespace Plus.Communication.Packets.Outgoing.Rooms.Settings
{
    using HabboHotel.Rooms;

    internal class GetRoomFilterListComposer : ServerPacket
    {
        public GetRoomFilterListComposer(Room Instance)
            : base(ServerPacketHeader.GetRoomFilterListMessageComposer)
        {
            WriteInteger(Instance.WordFilterList.Count);
            foreach (var Word in Instance.WordFilterList)
            {
               WriteString(Word);
            }
        }
    }
}
