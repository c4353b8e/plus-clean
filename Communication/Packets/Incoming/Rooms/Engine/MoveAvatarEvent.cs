namespace Plus.Communication.Packets.Incoming.Rooms.Engine
{
    using Game.Players;

    internal class MoveAvatarEvent : IPacketEvent
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

            var room = session.GetHabbo().CurrentRoom;
            if (room == null)
            {
                return;
            }

            var user = room.GetRoomUserManager().GetRoomUserByHabbo(session.GetHabbo().Id);
            if (user == null || !user.CanWalk)
            {
                return;
            }

            var moveX = packet.PopInt();
            var moveY = packet.PopInt();

            if (moveX == user.X && moveY == user.Y)
            {
                return;
            }

            if (user.RidingHorse)
            {
                var horse = room.GetRoomUserManager().GetRoomUserByVirtualId(user.HorseID);
                if (horse != null)
                {
                    horse.MoveTo(moveX, moveY);
                }
            }

            user.MoveTo(moveX, moveY);        
        }
    }
}