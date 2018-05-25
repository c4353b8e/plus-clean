namespace Plus.Communication.Packets.Incoming.Navigator
{
    using Game.Players;
    using Game.Rooms;
    using Outgoing.Navigator;

    internal class UpdateNavigatorSettingsEvent : IPacketEvent
    {
        public void Parse(Player session, ClientPacket packet)
        {
            var roomId = packet.PopInt();
            if (roomId == 0)
            {
                return;
            }

            if (!RoomFactory.TryGetData(roomId, out RoomData _))
            {
                return;
            }

            session.GetHabbo().HomeRoom = roomId;
            session.SendPacket(new NavigatorSettingsComposer(roomId));
        }
    }
}
