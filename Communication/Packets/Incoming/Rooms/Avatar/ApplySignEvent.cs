namespace Plus.Communication.Packets.Incoming.Rooms.Avatar
{
    using System;
    using HabboHotel.GameClients;
    using Utilities;

    internal class ApplySignEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
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
            user.SignTime = UnixTimestamp.GetNow() + 5;
        }
    }
}