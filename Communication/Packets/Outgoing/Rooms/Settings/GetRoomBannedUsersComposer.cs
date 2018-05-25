namespace Plus.Communication.Packets.Outgoing.Rooms.Settings
{
    using System.Linq;
    using HabboHotel.Rooms;

    internal class GetRoomBannedUsersComposer : ServerPacket
    {
        public GetRoomBannedUsersComposer(Room instance)
            : base(ServerPacketHeader.GetRoomBannedUsersMessageComposer)
        {
            WriteInteger(instance.Id);

            WriteInteger(instance.GetBans().BannedUsers().Count);//Count
            foreach (var Id in instance.GetBans().BannedUsers().ToList())
            {
                var Data = Program.GameContext.GetCacheManager().GenerateUser(Id);

                if (Data == null)
                {
                    WriteInteger(0);
                    WriteString("Unknown Error");
                }
                else
                {
                    WriteInteger(Data.Id);
                    WriteString(Data.Username);
                }
            }
        }
    }
}