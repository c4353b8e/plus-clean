namespace Plus.Communication.Packets.Incoming.Messenger
{
    using System.Collections.Generic;
    using System.Linq;
    using Game.Players;
    using Game.Users.Messenger;
    using MoreLinq;
    using Outgoing.Messenger;

    internal class MessengerInitEvent : IPacketEvent
    {
        public void Parse(Player session, ClientPacket packet)
        {
            if (session == null || session.GetHabbo() == null || session.GetHabbo().GetMessenger() == null)
            {
                return;
            }

            session.GetHabbo().GetMessenger().OnStatusChanged(false);

            ICollection<MessengerBuddy> friends = new List<MessengerBuddy>();
            foreach (var buddy in session.GetHabbo().GetMessenger().GetFriends().ToList())
            {
                if (buddy == null || buddy.IsOnline)
                {
                    continue;
                }

                friends.Add(buddy);
            }

            session.SendPacket(new MessengerInitComposer());

            var page = 0;
            if (!friends.Any())
            {
                session.SendPacket(new BuddyListComposer(friends, session.GetHabbo(), 1, 0));
            }
            else
            {
                var pages = (friends.Count() - 1) / 500 + 1;
                foreach (ICollection<MessengerBuddy> batch in friends.Batch(500))
                {
                    session.SendPacket(new BuddyListComposer(batch.ToList(), session.GetHabbo(), pages, page));

                    page++;
                }
            }
          
            session.GetHabbo().GetMessenger().ProcessOfflineMessages();
        }
    }
}