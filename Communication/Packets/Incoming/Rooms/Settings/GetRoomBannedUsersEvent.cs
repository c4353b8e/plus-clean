namespace Plus.Communication.Packets.Incoming.Rooms.Settings
{
    using Game.Players;
    using Outgoing.Rooms.Settings;

    internal class GetRoomBannedUsersEvent : IPacketEvent
    {
        public void Parse(Player session, ClientPacket packet)
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
