namespace Plus.Communication.Packets.Incoming.Rooms.Connection
{
    using Game.Players;
    using Outgoing.Rooms.Session;

    internal class GoToFlatEvent : IPacketEvent
    {
        public void Parse(Player session, ClientPacket packet)
        {
            if (!session.GetHabbo().InRoom)
            {
                return;
            }

            if (!session.GetHabbo().EnterRoom(session.GetHabbo().CurrentRoom))
            {
                session.SendPacket(new CloseConnectionComposer());
            }
        }
    }
}
