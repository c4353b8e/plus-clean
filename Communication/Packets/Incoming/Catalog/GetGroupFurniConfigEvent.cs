namespace Plus.Communication.Packets.Incoming.Catalog
{
    using Game.Players;
    using Outgoing.Catalog;

    internal class GetGroupFurniConfigEvent : IPacketEvent
    {
        public void Parse(Player session, ClientPacket packet)
        {
            session.SendPacket(new GroupFurniConfigComposer(Program.GameContext.GetGroupManager().GetGroupsForUser(session.GetHabbo().Id)));
        }
    }
}
