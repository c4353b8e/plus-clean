namespace Plus.Communication.Packets.Incoming.Navigator
{
    using Game.Players;
    using Outgoing.Navigator;

    internal class CanCreateRoomEvent : IPacketEvent
    {
        public void Parse(Player session, ClientPacket packet)
        {
            session.SendPacket(new CanCreateRoomComposer(false, 150));
        }
    }
}
