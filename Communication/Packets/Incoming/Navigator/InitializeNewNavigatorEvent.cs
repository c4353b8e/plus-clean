namespace Plus.Communication.Packets.Incoming.Navigator
{
    using Game.Players;
    using Outgoing.Navigator.New;

    internal class InitializeNewNavigatorEvent : IPacketEvent
    {
        public void Parse(Player session, ClientPacket packet)
        {
            var topLevelItems = Program.GameContext.GetNavigator().GetTopLevelItems();

            session.SendPacket(new NavigatorMetaDataParserComposer(topLevelItems));
            session.SendPacket(new NavigatorLiftedRoomsComposer());
            session.SendPacket(new NavigatorCollapsedCategoriesComposer());
            session.SendPacket(new NavigatorPreferencesComposer());
        }
    }
}
