namespace Plus.Communication.Packets.Incoming.Groups
{
    using Game.Players;
    using Game.Users.Authenticator;
    using Outgoing.Groups;

    internal class AcceptGroupMembershipEvent : IPacketEvent
    {
        public void Parse(Player session, ClientPacket packet)
        {
            var groupId = packet.PopInt();
            var userId = packet.PopInt();

            if (!Program.GameContext.GetGroupManager().TryGetGroup(groupId, out var group))
            {
                return;
            }

            if (session.GetHabbo().Id != group.CreatorId && !group.IsAdmin(session.GetHabbo().Id) && !session.GetHabbo().GetPermissions().HasRight("fuse_group_accept_any"))
            {
                return;
            }

            if (!group.HasRequest(userId))
            {
                return;
            }

            var habbo = HabboFactory.GetHabboById(userId);
            if (habbo == null)
            {
                session.SendNotification("Oops, an error occurred whilst finding this user.");
                return;
            }

            group.HandleRequest(userId, true);

            session.SendPacket(new GroupMemberUpdatedComposer(groupId, habbo, 4));
        }
    }
}