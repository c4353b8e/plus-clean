namespace Plus.Communication.Packets.Incoming.Rooms.Avatar
{
    using System;
    using Game.Players;
    using Utilities;

    internal class ApplySignEvent : IPacketEvent
    {
        public void Parse(Player session, ClientPacket packet)
        {
            var signId = packet.PopInt();
            if (!Program.GameContext.GetRoomManager().TryGetRoom(session.GetHabbo().CurrentRoomId, out var room))
            {
                return;
            }

            var user = room.GetRoomUserManager().GetRoomUserByHabbo(session.GetHabbo().Id);
            if (user == null)
            {
                return;
            }


            user.UnIdle();

            user.SetStatus("sign", Convert.ToString(signId));
            user.UpdateNeeded = true;
            user.SignTime = UnixUtilities.GetNow() + 5;
        }
    }
}