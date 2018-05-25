namespace Plus.Communication.Packets.Incoming.Inventory.Achievements
{
    using System.Linq;
    using HabboHotel.GameClients;
    using Outgoing.Inventory.Achievements;

    internal class GetAchievementsEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            session.SendPacket(new AchievementsComposer(session, Program.GameContext.GetAchievementManager().Achievements.Values.ToList()));
        }
    }
}
