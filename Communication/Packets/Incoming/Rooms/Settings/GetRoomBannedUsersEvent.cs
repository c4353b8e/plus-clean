namespace Plus.Communication.Packets.Incoming.Rooms.Settings
{
    using HabboHotel.GameClients;
    using Outgoing.Rooms.Settings;

    internal class GetRoomBannedUsersEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            if (!session.GetHabbo().InRoom)
            {
                return;
            }

            var instance = session.GetHabbo().CurrentRoom;
            if (instance == null || !instance.CheckRights(session, true))
            {
                return;
            }

            if (instance.GetBans().BannedUsers().Count > 0)
            {
                session.SendPacket(new GetRoomBannedUsersComposer(instance));
            }
        }
    }
}
