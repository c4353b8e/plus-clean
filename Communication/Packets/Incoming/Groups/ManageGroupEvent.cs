namespace Plus.Communication.Packets.Incoming.Groups
{
    using Game.Players;
    using Outgoing.Groups;

    internal class ManageGroupEvent : IPacketEvent
    {
        public void Parse(Player session, ClientPacket packet)
        {
            var groupId = packet.PopInt();

            if (!Program.GameContext.GetGroupManager().TryGetGroup(groupId, out var group))
            {
                return;
            }

            if (group.CreatorId != session.GetHabbo().Id && !session.GetHabbo().GetPermissions().HasRight("group_management_override"))
            {
                return;
            }

            session.SendPacket(new ManageGroupComposer(group, group.Badge.Replace("b", "").Split('s')));
        }
    }
}
