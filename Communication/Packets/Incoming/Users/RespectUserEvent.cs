namespace Plus.Communication.Packets.Incoming.Users
{
    using Game.Players;
    using Game.Quests;
    using Outgoing.Rooms.Avatar;
    using Outgoing.Users;

    internal class RespectUserEvent : IPacketEvent
    {
        public void Parse(Player session, ClientPacket packet)
        {
            if (session == null || session.GetHabbo() == null)
            {
                return;
            }

            if (!session.GetHabbo().InRoom || session.GetHabbo().GetStats().DailyRespectPoints <= 0)
            {
                return;
            }

            if (!Program.GameContext.GetRoomManager().TryGetRoom(session.GetHabbo().CurrentRoomId, out var room))
            {
                return;
            }

            var user = room.GetRoomUserManager().GetRoomUserByHabbo(packet.PopInt());
            if (user == null || user.GetClient() == null || user.GetClient().GetHabbo().Id == session.GetHabbo().Id || user.IsBot)
            {
                return;
            }

            var thisUser = room.GetRoomUserManager().GetRoomUserByHabbo(session.GetHabbo().Id);
            if (thisUser == null)
            {
                return;
            }

            Program.GameContext.GetQuestManager().ProgressUserQuest(session, QuestType.SocialRespect);
            Program.GameContext.GetAchievementManager().ProgressAchievement(session, "ACH_RespectGiven", 1);
            Program.GameContext.GetAchievementManager().ProgressAchievement(user.GetClient(), "ACH_RespectEarned", 1);

            session.GetHabbo().GetStats().DailyRespectPoints -= 1;
            session.GetHabbo().GetStats().RespectGiven += 1;
            user.GetClient().GetHabbo().GetStats().Respect += 1;

            if (room.RespectNotificationsEnabled)
            {
                room.SendPacket(new RespectNotificationComposer(user.GetClient().GetHabbo().Id, user.GetClient().GetHabbo().GetStats().Respect));
            }

            room.SendPacket(new ActionComposer(thisUser.VirtualId, 7));
        }
    }
}