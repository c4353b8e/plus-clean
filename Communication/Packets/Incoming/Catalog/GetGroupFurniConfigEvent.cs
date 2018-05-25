namespace Plus.Communication.Packets.Incoming.Catalog
{
    using HabboHotel.GameClients;
    using Outgoing.Catalog;

    internal class GetGroupFurniConfigEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            session.SendPacket(new GroupFurniConfigComposer(Program.GameContext.GetGroupManager().GetGroupsForUser(session.GetHabbo().Id)));
        }
    }
}
