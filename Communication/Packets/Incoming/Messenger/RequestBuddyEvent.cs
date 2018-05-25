namespace Plus.Communication.Packets.Incoming.Messenger
{
    using Game.Players;
    using Game.Quests;

    internal class RequestBuddyEvent : IPacketEvent
    {
        public void Parse(Player session, ClientPacket packet)
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
