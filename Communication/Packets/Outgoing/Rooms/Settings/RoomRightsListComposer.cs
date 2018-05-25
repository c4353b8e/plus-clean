namespace Plus.Communication.Packets.Outgoing.Rooms.Settings
{
    using System.Linq;
    using Game.Rooms;

    internal class RoomRightsListComposer : ServerPacket
    {
        public RoomRightsListComposer(Room Instance)
            : base(ServerPacketHeader.RoomRightsListMessageComposer)
        {
            WriteInteger(Instance.Id);

            WriteInteger(Instance.UsersWithRights.Count);
            foreach (var Id in Instance.UsersWithRights.ToList())
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
