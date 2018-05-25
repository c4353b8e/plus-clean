namespace Plus.Communication.Packets.Incoming.Groups
{
    using Game.Players;
    using Outgoing.Groups;
    using Outgoing.Users;

    internal class SetGroupFavouriteEvent : IPacketEvent
    {
        public void Parse(Player session, ClientPacket packet)
        {
            if (session == null)
            {
                return;
            }

            var groupId = packet.PopInt();
            if (groupId == 0)
            {
                return;
            }

            if (!Program.GameContext.GetGroupManager().TryGetGroup(groupId, out var group))
            {
                return;
            }

            session.GetHabbo().GetStats().FavouriteGroupId = group.Id;
            using (var dbClient = Program.DatabaseManager.GetQueryReactor())
            {
                dbClient.SetQuery("UPDATE `user_stats` SET `groupid` = @groupId WHERE `id` = @userId LIMIT 1");
                dbClient.AddParameter("groupId", session.GetHabbo().GetStats().FavouriteGroupId);
                dbClient.AddParameter("userId", session.GetHabbo().Id);
                dbClient.RunQuery();
            }

            if (session.GetHabbo().InRoom && session.GetHabbo().CurrentRoom != null)
            {
                session.GetHabbo().CurrentRoom.SendPacket(new RefreshFavouriteGroupComposer(session.GetHabbo().Id));
                session.GetHabbo().CurrentRoom.SendPacket(new HabboGroupBadgesComposer(group));

                var user = session.GetHabbo().CurrentRoom.GetRoomUserManager()
                    .GetRoomUserByHabbo(session.GetHabbo().Id);
                if (user != null)
                {
                    session.GetHabbo().CurrentRoom.SendPacket(new UpdateFavouriteGroupComposer(group, user.VirtualId));
                }
            }
            else
            {
                session.SendPacket(new RefreshFavouriteGroupComposer(session.GetHabbo().Id));
            }
        }
    }
}
