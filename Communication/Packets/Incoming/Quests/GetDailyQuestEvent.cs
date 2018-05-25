namespace Plus.Communication.Packets.Incoming.Quests
{
    using Game.Players;
    using Outgoing.LandingView;

    internal class GetDailyQuestEvent : IPacketEvent
    {
        public void Parse(Player session, ClientPacket packet)
        {
            var usersOnline = Program.GameContext.PlayerController.Count;

            session.SendPacket(new ConcurrentUsersGoalProgressComposer(usersOnline));
        }
    }
}
