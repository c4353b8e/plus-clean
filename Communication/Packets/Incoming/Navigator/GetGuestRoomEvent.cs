namespace Plus.Communication.Packets.Incoming.Navigator
{
    using Game.Players;
    using Game.Rooms;
    using Outgoing.Navigator;

    internal class GetGuestRoomEvent : IPacketEvent
    {
        public void Parse(Player session, ClientPacket packet)
        {
            var roomId = packet.PopInt();

            if (!RoomFactory.TryGetData(roomId, out var data))
            {
                return;
            }

            var enter = packet.PopInt() == 1;
            var forward = packet.PopInt() == 1;

            session.SendPacket(new GetGuestRoomResultComposer(session, data, enter, forward));
        }
    }
}
