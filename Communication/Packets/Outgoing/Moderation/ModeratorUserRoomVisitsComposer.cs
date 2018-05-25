namespace Plus.Communication.Packets.Outgoing.Moderation
{
    using System.Collections.Generic;
    using HabboHotel.Rooms;
    using HabboHotel.Users;
    using Utilities;

    internal class ModeratorUserRoomVisitsComposer : ServerPacket
    {
        public ModeratorUserRoomVisitsComposer(Habbo Data, Dictionary<double, RoomData> Visits)
            : base(ServerPacketHeader.ModeratorUserRoomVisitsMessageComposer)
        {
            WriteInteger(Data.Id);
            WriteString(Data.Username);
            WriteInteger(Visits.Count);

            foreach (var Visit in Visits)
            {
                WriteInteger(Visit.Value.Id);
                WriteString(Visit.Value.Name);
                WriteInteger(UnixTimestamp.FromUnixTimestamp(Visit.Key).Hour);
                WriteInteger(UnixTimestamp.FromUnixTimestamp(Visit.Key).Minute);
            }
        }
    }
}
