namespace Plus.Communication.Packets.Incoming.Navigator
{
    using System.Collections.Generic;
    using Game.Navigator;
    using Game.Players;
    using Outgoing.Navigator.New;

    internal class NavigatorSearchEvent : IPacketEvent
    {
        public void Parse(Player session, ClientPacket packet)
        {
            var category = packet.PopString();
            var search = packet.PopString();

            ICollection<SearchResultList> categories = new List<SearchResultList>();

            if (!string.IsNullOrEmpty(search))
            {
                if (Program.GameContext.GetNavigator().TryGetSearchResultList(0, out var queryResult))
                {
                    categories.Add(queryResult);
                }
            }
            else
            {
                categories = Program.GameContext.GetNavigator().GetCategorysForSearch(category);
                if (categories.Count == 0)
                {
                    //Are we going in deep?!
                    categories = Program.GameContext.GetNavigator().GetResultByIdentifier(category);
                    if (categories.Count > 0)
                    {
                        session.SendPacket(new NavigatorSearchResultSetComposer(category, search, categories, session, 2, 100));
                        return;
                    }
                }
            }

            session.SendPacket(new NavigatorSearchResultSetComposer(category, search, categories, session));
        }
    }
}
