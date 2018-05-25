namespace Plus.Communication.Packets.Incoming.Groups
{
    using System.Collections.Generic;
    using System.Linq;
    using Game.Cache.Type;
    using Game.Players;
    using Outgoing.Groups;
    using Outgoing.Rooms.Permissions;

    internal class RemoveGroupMemberEvent : IPacketEvent
    {
        public void Parse(Player session, ClientPacket packet)
        {
            var groupId = packet.PopInt();
            var userId = packet.PopInt();

            if (!Program.GameContext.GetGroupManager().TryGetGroup(groupId, out var group))
            {
                return;
            }

            if (userId == session.GetHabbo().Id)
            {
                if (group.IsMember(userId))
                {
                    group.DeleteMember(userId);
                }

                if (group.IsAdmin(userId))
                {
                    if (group.IsAdmin(userId))
                    {
                        group.TakeAdmin(userId);
                    }

                    if (!Program.GameContext.GetRoomManager().TryGetRoom(group.RoomId, out var room))
                    {
                        return;
                    }

                    var user = room.GetRoomUserManager().GetRoomUserByHabbo(session.GetHabbo().Id);
                    if (user != null)
                    {
                        user.RemoveStatus("flatctrl 1");
                        user.UpdateNeeded = true;

                        if (user.GetClient() != null)
                        {
                            user.GetClient().SendPacket(new YouAreControllerComposer(0));
                        }
                    }
                }

                using (var dbClient = Program.DatabaseManager.GetQueryReactor())
                {
                    dbClient.SetQuery(
                        "DELETE FROM `group_memberships` WHERE `group_id` = @GroupId AND `user_id` = @UserId");
                    dbClient.AddParameter("GroupId", groupId);
                    dbClient.AddParameter("UserId", userId);
                    dbClient.RunQuery();
                }

                session.SendPacket(new GroupInfoComposer(group, session));
                if (session.GetHabbo().GetStats().FavouriteGroupId == groupId)
                {
                    session.GetHabbo().GetStats().FavouriteGroupId = 0;
                    using (var dbClient = Program.DatabaseManager.GetQueryReactor())
                    {
                        dbClient.SetQuery("UPDATE `user_stats` SET `groupid` = '0' WHERE `id` = @userId LIMIT 1");
                        dbClient.AddParameter("userId", userId);
                        dbClient.RunQuery();
                    }

                    if (group.AdminOnlyDeco == 0)
                    {
                        if (!Program.GameContext.GetRoomManager().TryGetRoom(group.RoomId, out var room))
                        {
                            return;
                        }

                        var user = room.GetRoomUserManager().GetRoomUserByHabbo(session.GetHabbo().Id);
                        if (user != null)
                        {
                            user.RemoveStatus("flatctrl 1");
                            user.UpdateNeeded = true;

                            if (user.GetClient() != null)
                            {
                                user.GetClient().SendPacket(new YouAreControllerComposer(0));
                            }
                        }
                    }

                    if (session.GetHabbo().InRoom && session.GetHabbo().CurrentRoom != null)
                    {
                        var user = session.GetHabbo().CurrentRoom.GetRoomUserManager()
                            .GetRoomUserByHabbo(session.GetHabbo().Id);
                        if (user != null)
                        {
                            session.GetHabbo().CurrentRoom
                                .SendPacket(new UpdateFavouriteGroupComposer(group, user.VirtualId));
                        }

                        session.GetHabbo().CurrentRoom
                            .SendPacket(new RefreshFavouriteGroupComposer(session.GetHabbo().Id));
                    }
                    else
                    {
                        session.SendPacket(new RefreshFavouriteGroupComposer(session.GetHabbo().Id));
                    }
                }

                return;
            }

            if (group.CreatorId == session.GetHabbo().Id || group.IsAdmin(session.GetHabbo().Id))
            {
                if (!group.IsMember(userId))
                {
                    return;
                }

                if (group.IsAdmin(userId) && group.CreatorId != session.GetHabbo().Id)
                {
                    session.SendNotification(
                        "Sorry, only group creators can remove other administrators from the group.");
                    return;
                }

                if (group.IsAdmin(userId))
                {
                    group.TakeAdmin(userId);
                }

                if (group.IsMember(userId))
                {
                    group.DeleteMember(userId);
                }

                var members = new List<UserCache>();
                var memberIds = group.GetAllMembers;
                foreach (var id in memberIds.ToList())
                {
                    var groupMember = Program.GameContext.GetCacheManager().GenerateUser(id);
                    if (groupMember == null)
                    {
                        continue;
                    }

                    if (!members.Contains(groupMember))
                    {
                        members.Add(groupMember);
                    }
                }


                var finishIndex = 14 < members.Count ? 14 : members.Count;
                var membersCount = members.Count;

                session.SendPacket(new GroupMembersComposer(group, members.Take(finishIndex).ToList(), membersCount, 1,
                    group.CreatorId == session.GetHabbo().Id || group.IsAdmin(session.GetHabbo().Id), 0, ""));
            }
        }
    }
}