namespace Plus.Communication.Packets.Incoming.Groups
{
    using HabboHotel.GameClients;
    using HabboHotel.Users.Authenticator;
    using Outgoing.Groups;
    using Outgoing.Rooms.Permissions;

    internal class TakeAdminRightsEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            var groupId = packet.PopInt();
            var userId = packet.PopInt();

            if (!Program.GameContext.GetGroupManager().TryGetGroup(groupId, out var group))
            {
                return;
            }

            if (session.GetHabbo().Id != group.CreatorId || !group.IsMember(userId))
            {
                return;
            }

            var habbo = HabboFactory.GetHabboById(userId);
            if (habbo == null)
            {
                session.SendNotification("Oops, an error occurred whilst finding this user.");
                return;
            }

            group.TakeAdmin(userId);

            if (Program.GameContext.GetRoomManager().TryGetRoom(group.RoomId, out var room))
            {
                var user = room.GetRoomUserManager().GetRoomUserByHabbo(userId);
                if (user != null)
                {
                    if (user.Statusses.ContainsKey("flatctrl 3"))
                    {
                        user.RemoveStatus("flatctrl 3");
                    }

                    user.UpdateNeeded = true;
                    if (user.GetClient() != null)
                    {
                        user.GetClient().SendPacket(new YouAreControllerComposer(0));
                    }
                }
            }

            session.SendPacket(new GroupMemberUpdatedComposer(groupId, habbo, 2));
        }
    }
}
