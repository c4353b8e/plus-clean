namespace Plus.Communication.Packets.Incoming.Rooms.Avatar
{
    using HabboHotel.GameClients;
    using HabboHotel.Quests;
    using Outgoing.Rooms.Avatar;

    internal class DanceEvent : IPacketEvent
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

            user.UnIdle();

            var danceId = packet.PopInt();
            if (danceId < 0 || danceId > 4)
            {
                danceId = 0;
            }

            if (danceId > 0 && user.CarryItemId > 0)
            {
                user.CarryItem(0);
            }

            if (session.GetHabbo().Effects().CurrentEffect > 0)
            {
                room.SendPacket(new AvatarEffectComposer(user.VirtualId, 0));
            }

            user.DanceId = danceId;

            room.SendPacket(new DanceComposer(user, danceId));

            Program.GameContext.GetQuestManager().ProgressUserQuest(session, QuestType.SocialDance);
            if (room.GetRoomUserManager().GetRoomUsers().Count > 19)
            {
                Program.GameContext.GetQuestManager().ProgressUserQuest(session, QuestType.MassDance);
            }
        }
    }
}