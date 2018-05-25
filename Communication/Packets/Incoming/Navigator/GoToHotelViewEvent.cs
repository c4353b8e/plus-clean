namespace Plus.Communication.Packets.Incoming.Navigator
{
    using Game.Players;

    internal class GoToHotelViewEvent : IPacketEvent
    {
        public void Parse(Player session, ClientPacket packet)
        {
            if (session == null || session.GetHabbo() == null)
            {
                return;
            }
            
            if (!session.GetHabbo().InRoom)
            {
                return;
            }

            if (!Program.GameContext.GetRoomManager().TryGetRoom(session.GetHabbo().CurrentRoomId, out var oldRoom))
            {
                return;
            }

            if (oldRoom.GetRoomUserManager() != null)
            {
                oldRoom.GetRoomUserManager().RemoveUserFromRoom(session, true);
            }
        }
    }
}
