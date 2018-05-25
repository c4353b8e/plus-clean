namespace Plus.Communication.Packets.Incoming.Navigator
{
    using HabboHotel.GameClients;
    using Outgoing.Rooms.Session;

    internal class FindRandomFriendingRoomEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            var instance = Program.GameContext.GetRoomManager().TryGetRandomLoadedRoom();

            if (instance != null)
            {
                session.SendPacket(new RoomForwardComposer(instance.Id));
            }
        }
    }
}
