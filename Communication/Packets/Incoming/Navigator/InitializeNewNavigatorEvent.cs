namespace Plus.Communication.Packets.Incoming.Navigator
{
    using HabboHotel.GameClients;
    using Outgoing.Navigator.New;

    internal class InitializeNewNavigatorEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            var topLevelItems = Program.GameContext.GetNavigator().GetTopLevelItems();

            session.SendPacket(new NavigatorMetaDataParserComposer(topLevelItems));
            session.SendPacket(new NavigatorLiftedRoomsComposer());
            session.SendPacket(new NavigatorCollapsedCategoriesComposer());
            session.SendPacket(new NavigatorPreferencesComposer());
        }
    }
}
