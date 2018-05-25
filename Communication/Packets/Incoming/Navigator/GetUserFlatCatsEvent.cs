namespace Plus.Communication.Packets.Incoming.Navigator
{
    using HabboHotel.GameClients;
    using Outgoing.Navigator;

    public class GetUserFlatCatsEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
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