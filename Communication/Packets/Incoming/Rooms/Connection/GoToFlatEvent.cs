namespace Plus.Communication.Packets.Incoming.Rooms.Connection
{
    using HabboHotel.GameClients;
    using Outgoing.Rooms.Session;

    internal class GoToFlatEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
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
