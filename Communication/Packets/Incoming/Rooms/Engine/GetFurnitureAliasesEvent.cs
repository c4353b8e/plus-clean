namespace Plus.Communication.Packets.Incoming.Rooms.Engine
{
    using HabboHotel.GameClients;
    using Outgoing.Rooms.Engine;

    internal class GetFurnitureAliasesEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            session.SendPacket(new FurnitureAliasesComposer());
        }
    }
}
