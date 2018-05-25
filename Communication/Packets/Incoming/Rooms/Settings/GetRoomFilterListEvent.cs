namespace Plus.Communication.Packets.Incoming.Rooms.Settings
{
    using HabboHotel.GameClients;
    using Outgoing.Rooms.Settings;

    internal class GetRoomFilterListEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            if (!session.GetHabbo().InRoom)
            {
                return;
            }

            var instance = session.GetHabbo().CurrentRoom;
            if (instance == null)
            {
                return;
            }

            if (!instance.CheckRights(session))
            {
                return;
            }

            session.SendPacket(new GetRoomFilterListComposer(instance));
            Program.GameContext.GetAchievementManager().ProgressAchievement(session, "ACH_SelfModRoomFilterSeen", 1);
        }
    }
}
