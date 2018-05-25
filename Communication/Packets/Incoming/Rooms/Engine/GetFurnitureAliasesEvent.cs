namespace Plus.Communication.Packets.Incoming.Rooms.Engine
{
    using Game.Players;
    using Outgoing.Rooms.Engine;

    internal class GetFurnitureAliasesEvent : IPacketEvent
    {
        public void Parse(Player session, ClientPacket packet)
        {
            session.SendPacket(new FurnitureAliasesComposer());
        }
    }
}
