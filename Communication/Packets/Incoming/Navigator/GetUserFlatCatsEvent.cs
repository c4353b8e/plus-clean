namespace Plus.Communication.Packets.Incoming.Navigator
{
    using Game.Players;
    using Outgoing.Navigator;

    public class GetUserFlatCatsEvent : IPacketEvent
    {
        public void Parse(Player session, ClientPacket packet)
        {
            if (session == null)
            {
                return;
            }

            var categories = Program.GameContext.GetNavigator().GetFlatCategories();

            session.SendPacket(new UserFlatCatsComposer(categories, session.GetHabbo().Rank));
        }
    }
}