namespace Plus.Communication.Packets.Incoming.Rooms.Avatar
{
    using HabboHotel.GameClients;

    internal class DropHandItemEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            if (!session.GetHabbo().InRoom)
            {
                return;
            }

            if (!Program.GameContext.GetRoomManager().TryGetRoom(session.GetHabbo().CurrentRoomId, out var room))
            {
                return;
            }

            var user = room.GetRoomUserManager().GetRoomUserByHabbo(session.GetHabbo().Id);
            if (user == null)
            {
                return;
            }

            if (user.CarryItemId > 0 && user.CarryTimer > 0)
            {
                user.CarryItem(0);
            }
        }
    }
}
