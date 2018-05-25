namespace Plus.Communication.Packets.Incoming.Rooms.Avatar
{
    using Game.Players;
    using Game.Rooms.PathFinding;

    internal class LookToEvent : IPacketEvent
    {
        public void Parse(Player session, ClientPacket packet)
        {
            if (!Program.GameContext.GetRoomManager().TryGetRoom(session.GetHabbo().CurrentRoomId, out var room))
            {
                return;
            }

            var user = room.GetRoomUserManager().GetRoomUserByHabbo(session.GetHabbo().Id);
            if (user == null)
            {
                return;
            }

            if (user.IsAsleep)
            {
                return;
            }

            user.UnIdle();

            var x = packet.PopInt();
            var y = packet.PopInt();

            if (x == user.X && y == user.Y || user.IsWalking || user.RidingHorse)
            {
                return;
            }

            var rot = Rotation.Calculate(user.X, user.Y, x, y);

            user.SetRot(rot, false);
            user.UpdateNeeded = true;

            if (user.RidingHorse)
            {
                var horse = session.GetHabbo().CurrentRoom.GetRoomUserManager().GetRoomUserByVirtualId(user.HorseID);
                if (horse != null)
                {
                    horse.SetRot(rot, false);
                    horse.UpdateNeeded = true;
                }
            }
        }
    }
}
