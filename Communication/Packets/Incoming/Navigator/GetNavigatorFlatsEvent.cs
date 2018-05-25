namespace Plus.Communication.Packets.Incoming.Navigator
{
    using Game.Players;
    using Outgoing.Navigator;

    internal class GetNavigatorFlatsEvent : IPacketEvent
    {
        public void Parse(Player session, ClientPacket packet)
        {
            var categories = Program.GameContext.GetNavigator().GetEventCategories();

            session.SendPacket(new NavigatorFlatCatsComposer(categories));
        }
    }
}