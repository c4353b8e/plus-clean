namespace Plus.Communication.Packets.Incoming.Groups
{
    using System.Linq;
    using HabboHotel.GameClients;
    using HabboHotel.Groups;
    using Outgoing.Groups;
    using Outgoing.Rooms.Permissions;

    internal class UpdateGroupSettingsEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            var groupId = packet.PopInt();

            if (!Program.GameContext.GetGroupManager().TryGetGroup(groupId, out var group))
            {
                return;
            }

            if (group.CreatorId != session.GetHabbo().Id)
            {
                return;
            }

            var type = packet.PopInt();
            var furniOptions = packet.PopInt();

            switch (type)
            {
                default:
                    group.Type = GroupType.Open;
                    break;
                case 1:
                    group.Type = GroupType.Locked;
                    break;
                case 2:
                    group.Type = GroupType.Private;
                    break;
            }

            if (group.Type != GroupType.Locked)
            {
                if (group.GetRequests.Count > 0)
                {
                    foreach (var userId in group.GetRequests.ToList())
                    {
                        group.HandleRequest(userId, false);
                    }

                    group.ClearRequests();
                }
            }

            using (var dbClient = Program.DatabaseManager.GetQueryReactor())
            {
                dbClient.SetQuery("UPDATE `groups` SET `state` = @GroupState, `admindeco` = @AdminDeco WHERE `id` = @groupId LIMIT 1");
                dbClient.AddParameter("GroupState", (group.Type == GroupType.Open ? 0 : group.Type == GroupType.Locked ? 1 : 2).ToString());
                dbClient.AddParameter("AdminDeco", (furniOptions == 1 ? 1 : 0).ToString());
                dbClient.AddParameter("groupId", group.Id);
                dbClient.RunQuery();
            }

            group.AdminOnlyDeco = furniOptions;

            if (!Program.GameContext.GetRoomManager().TryGetRoom(group.RoomId, out var room))
            {
                return;
            }

            foreach (var user in room.GetRoomUserManager().GetRoomUsers().ToList())
            {
                if (room.OwnerId == user.UserId || group.IsAdmin(user.UserId) || !group.IsMember(user.UserId))
                {
                    continue;
                }

                if (furniOptions == 1)
                {
                    user.RemoveStatus("flatctrl 1");
                    user.UpdateNeeded = true;

                    user.GetClient().SendPacket(new YouAreControllerComposer(0));
                }
                else if (furniOptions == 0 && !user.Statusses.ContainsKey("flatctrl 1"))
                {
                    user.SetStatus("flatctrl 1", "");
                    user.UpdateNeeded = true;

                    user.GetClient().SendPacket(new YouAreControllerComposer(1));
                }
            }

            session.SendPacket(new GroupInfoComposer(group, session));
        }
    }
}