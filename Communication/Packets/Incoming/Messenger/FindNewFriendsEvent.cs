namespace Plus.Communication.Packets.Incoming.Messenger
{
    using Game.Players;
    using Outgoing.Messenger;
    using Outgoing.Rooms.Session;

    internal class FindNewFriendsEvent : IPacketEvent
    {
        public void Parse(Player session, ClientPacket packet)
        {
            var instance = Program.GameContext.GetRoomManager().TryGetRandomLoadedRoom();

            if (instance != null)
            {
                session.SendPacket(new FindFriendsProcessResultComposer(true));
                session.SendPacket(new RoomForwardComposer(instance.Id));
            }
            else
            {
                session.SendPacket(new FindFriendsProcessResultComposer(false));
            }
        }
    }
}
