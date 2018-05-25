namespace Plus.Communication.Packets.Incoming.Messenger
{
    using System.Collections.Generic;
    using System.Linq;
    using HabboHotel.GameClients;
    using HabboHotel.Users.Messenger;
    using Outgoing.Messenger;
    using Utilities;

    internal class HabboSearchEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            if (session == null || session.GetHabbo() == null || session.GetHabbo().GetMessenger() == null)
            {
                return;
            }

            var query = StringCharFilter.Escape(packet.PopString().Replace("%", ""));
            if (query.Length < 1 || query.Length > 100)
            {
                return;
            }

            var friends = new List<SearchResult>();
            var othersUsers = new List<SearchResult>();

            var results = SearchResultFactory.GetSearchResult(query);
            foreach (var result in results.ToList())
            {
                if (session.GetHabbo().GetMessenger().FriendshipExists(result.UserId))
                {
                    friends.Add(result);
                }
                else
                {
                    othersUsers.Add(result);
                }
            }

            session.SendPacket(new HabboSearchResultComposer(friends, othersUsers));
        }
    }
}