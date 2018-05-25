namespace Plus.Communication.Packets.Incoming.Navigator
{
    using HabboHotel.GameClients;

    internal class GoToHotelViewEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            if (session == null || session.GetHabbo() == null)
            {
                return;
            }


            if (session.GetHabbo().InRoom)
            {
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
}
