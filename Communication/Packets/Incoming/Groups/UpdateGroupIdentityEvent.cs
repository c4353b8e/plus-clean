namespace Plus.Communication.Packets.Incoming.Groups
{
    using Game.Players;
    using Outgoing.Groups;

    internal class UpdateGroupIdentityEvent : IPacketEvent
    {
        public void Parse(Player session, ClientPacket packet)
        {
            var groupId = packet.PopInt();
            var name = Program.GameContext.GetChatManager().GetFilter().CheckMessage(packet.PopString());
            var desc = Program.GameContext.GetChatManager().GetFilter().CheckMessage(packet.PopString());

            if (!Program.GameContext.GetGroupManager().TryGetGroup(groupId, out var group))
            {
                return;
            }

            if (group.CreatorId != session.GetHabbo().Id)
            {
                return;
            }

            using (var dbClient = Program.DatabaseManager.GetQueryReactor())
            {
                dbClient.SetQuery("UPDATE `groups` SET `name`= @name, `desc` = @desc WHERE `id` = @groupId LIMIT 1");
                dbClient.AddParameter("name", name);
                dbClient.AddParameter("desc", desc);
                dbClient.AddParameter("groupId", groupId);
                dbClient.RunQuery();
            }

            group.Name = name;
            group.Description = desc;

            session.SendPacket(new GroupInfoComposer(group, session));
        }
    }
}
