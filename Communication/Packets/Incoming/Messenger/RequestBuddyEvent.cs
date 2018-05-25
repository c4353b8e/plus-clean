namespace Plus.Communication.Packets.Incoming.Messenger
{
    using HabboHotel.GameClients;
    using HabboHotel.Quests;

    internal class RequestBuddyEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            if (session == null || session.GetHabbo() == null || session.GetHabbo().GetMessenger() == null)
            {
                return;
            }

            if (session.GetHabbo().GetMessenger().RequestBuddy(packet.PopString()))
            {
                Program.GameContext.GetQuestManager().ProgressUserQuest(session, QuestType.SocialFriend);
            }
        }
    }
}
