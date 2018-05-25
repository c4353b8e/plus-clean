namespace Plus.Communication.Packets.Incoming.Inventory.Achievements
{
    using System.Linq;
    using Game.Players;
    using Outgoing.Inventory.Achievements;

    internal class GetAchievementsEvent : IPacketEvent
    {
        public void Parse(Player session, ClientPacket packet)
        {
            session.SendPacket(new AchievementsComposer(session, Program.GameContext.GetAchievementManager().Achievements.Values.ToList()));
        }
    }
}
