namespace Plus.Communication.Packets.Incoming.Rooms.Action
{
    using Game.Players;

    internal class KickUserEvent : IPacketEvent
    {
        public void Parse(Player session, ClientPacket packet)
        {
            var room = session.GetHabbo().CurrentRoom;
            if (room == null)
            {
                return;
            }

            if (!room.CheckRights(session) && room.WhoCanKick != 2 && room.Group == null)
            {
                return;
            }

            if (room.Group != null && !room.CheckRights(session, false, true))
            {
                return;
            }

            var userId = packet.PopInt();
            var user = room.GetRoomUserManager().GetRoomUserByHabbo(userId);
            if (user == null || user.IsBot)
            {
                return;
            }

            //Cannot kick owner or moderators.
            if (room.CheckRights(user.GetClient(), true) || user.GetClient().GetHabbo().GetPermissions().HasRight("mod_tool"))
            {
                return;
            }

            room.GetRoomUserManager().RemoveUserFromRoom(user.GetClient(), true, true);
            Program.GameContext.GetAchievementManager().ProgressAchievement(session, "ACH_SelfModKickSeen", 1);
        }
    }
}
