namespace Plus.Communication.Packets.Incoming.Navigator
{
    using Game.Players;
    using Outgoing.Rooms.Session;

    internal class FindRandomFriendingRoomEvent : IPacketEvent
    {
        public void Parse(Player session, ClientPacket packet)
        {
            var instance = Program.GameContext.GetRoomManager().TryGetRandomLoadedRoom();

            if (instance != null)
            {
                session.SendPacket(new RoomForwardComposer(instance.Id));
            }
        }
    }
}
