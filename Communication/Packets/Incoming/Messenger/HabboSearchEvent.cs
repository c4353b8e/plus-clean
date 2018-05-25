namespace Plus.Communication.Packets.Incoming.Messenger
{
    using System.Collections.Generic;
    using System.Linq;
    using Game.Players;
    using Game.Users.Messenger;
    using Outgoing.Messenger;
    using Utilities;

    internal class HabboSearchEvent : IPacketEvent
    {
        public void Parse(Player session, ClientPacket packet)
        {
            if (session == null || session.GetHabbo() == null || session.GetHabbo().GetMessenger() == null)
            {
                return;
            }

            var query = StringUtilities.Escape(packet.PopString().Replace("%", ""));
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