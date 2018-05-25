namespace Plus.Communication.Packets.Incoming.Rooms.Action
{
    using System;
    using HabboHotel.GameClients;
    using HabboHotel.Quests;

    internal class GiveHandItemEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            if (session == null || session.GetHabbo() == null || !session.GetHabbo().InRoom)
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

            var targetUser = room.GetRoomUserManager().GetRoomUserByHabbo(packet.PopInt());
            if (targetUser == null)
            {
                return;
            }

            if (!(Math.Abs(user.X - targetUser.X) >= 3 || Math.Abs(user.Y - targetUser.Y) >= 3) || session.GetHabbo().GetPermissions().HasRight("mod_tool"))
            {
                if (user.CarryItemId > 0 && user.CarryTimer > 0)
                {
                    if (user.CarryItemId == 8)
                    {
                        Program.GameContext.GetQuestManager().ProgressUserQuest(session, QuestType.GiveCoffee);
                    }

                    targetUser.CarryItem(user.CarryItemId);
                    user.CarryItem(0);
                    targetUser.DanceId = 0;
                }
            }
        }
    }
}
