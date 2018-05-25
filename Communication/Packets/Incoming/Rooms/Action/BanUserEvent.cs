namespace Plus.Communication.Packets.Incoming.Rooms.Action
{
    using System;
    using HabboHotel.GameClients;

    internal class BanUserEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            var room = session.GetHabbo().CurrentRoom;
            if (room == null)
            {
                return;
            }

            if (room.WhoCanBan == 0 && !room.CheckRights(session, true) && room.Group == null || room.WhoCanBan == 1 && !room.CheckRights(session) && room.Group == null || room.Group != null && !room.CheckRights(session, false, true))
            {
                return;
            }

            var userId = packet.PopInt();
            packet.PopInt(); //roomId
            var r = packet.PopString();

            var user = room.GetRoomUserManager().GetRoomUserByHabbo(Convert.ToInt32(userId));
            if (user == null || user.IsBot)
            {
                return;
            }

            if (room.OwnerId == userId)
            {
                return;
            }

            if (user.GetClient().GetHabbo().GetPermissions().HasRight("mod_tool"))
            {
                return;
            }

            long time = 0;
            if (r.ToLower().Contains("hour"))
            {
                time = 3600;
            }
            else if (r.ToLower().Contains("day"))
            {
                time = 86400;
            }
            else if (r.ToLower().Contains("perm"))
            {
                time = 78892200;
            }

            room.GetBans().Ban(user, time);

            Program.GameContext.GetAchievementManager().ProgressAchievement(session, "ACH_SelfModBanSeen", 1);
        }
    }
}