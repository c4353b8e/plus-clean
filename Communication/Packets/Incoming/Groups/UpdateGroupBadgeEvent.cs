namespace Plus.Communication.Packets.Incoming.Groups
{
    using HabboHotel.GameClients;
    using HabboHotel.Groups;
    using Outgoing.Groups;

    internal class UpdateGroupBadgeEvent : IPacketEvent
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

            var count = packet.PopInt();

            var badge = "";
            for (var i = 0; i < count; i++)
            {
                badge += BadgePartUtility.WorkBadgeParts(i == 0, packet.PopInt().ToString(), packet.PopInt().ToString(), packet.PopInt().ToString());
            }

            group.Badge = string.IsNullOrWhiteSpace(badge) ? "b05114s06114" : badge;

            using (var dbClient = Program.DatabaseManager.GetQueryReactor())
            {
                dbClient.SetQuery("UPDATE `groups` SET `badge` = @badge WHERE `id` = @groupId LIMIT 1");
                dbClient.AddParameter("badge", group.Badge);
                dbClient.AddParameter("groupId", group.Id);
                dbClient.RunQuery();
            }

            session.SendPacket(new GroupInfoComposer(group, session));
        }
    }
}