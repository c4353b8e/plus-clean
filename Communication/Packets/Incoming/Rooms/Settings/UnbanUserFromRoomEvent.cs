namespace Plus.Communication.Packets.Incoming.Rooms.Settings
{
    using Game.Players;
    using Outgoing.Rooms.Settings;

    internal class UnbanUserFromRoomEvent : IPacketEvent
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

            var userId = packet.PopInt();
            var roomId = packet.PopInt();

            if (instance.GetBans().IsBanned(userId))
            {
                instance.GetBans().Unban(userId);

                session.SendPacket(new UnbanUserFromRoomComposer(roomId, userId));
            }
        }
    }
}