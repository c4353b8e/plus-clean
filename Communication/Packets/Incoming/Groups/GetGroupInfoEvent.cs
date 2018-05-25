namespace Plus.Communication.Packets.Incoming.Groups
{
    using HabboHotel.GameClients;
    using Outgoing.Groups;

    internal class GetGroupInfoEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            var groupId = packet.PopInt();
            var newWindow = packet.PopBoolean();

            if (!Program.GameContext.GetGroupManager().TryGetGroup(groupId, out var group))
            {
                return;
            }

            session.SendPacket(new GroupInfoComposer(group, session, newWindow));     
        }
    }
}
