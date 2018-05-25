namespace Plus.Communication.Packets.Incoming.Rooms.Avatar
{
    using HabboHotel.GameClients;
    using HabboHotel.Quests;
    using Outgoing.Rooms.Avatar;

    public class ActionEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            if (!session.GetHabbo().InRoom)
            {
                return;
            }

            var action = packet.PopInt();

            if (!Program.GameContext.GetRoomManager().TryGetRoom(session.GetHabbo().CurrentRoomId, out var room))
            {
                return;
            }

            var user = room.GetRoomUserManager().GetRoomUserByHabbo(session.GetHabbo().Id);
            if (user == null)
            {
                return;
            }

            if (user.DanceId > 0)
            {
                user.DanceId = 0;
            }

            if (session.GetHabbo().Effects().CurrentEffect > 0)
            {
                room.SendPacket(new AvatarEffectComposer(user.VirtualId, 0));
            }

            user.UnIdle();
            room.SendPacket(new ActionComposer(user.VirtualId, action));

            if (action == 5) // idle
            {
                user.IsAsleep = true;
                room.SendPacket(new SleepComposer(user, true));
            }

            Program.GameContext.GetQuestManager().ProgressUserQuest(session, QuestType.SocialWave);
        }
    }
}