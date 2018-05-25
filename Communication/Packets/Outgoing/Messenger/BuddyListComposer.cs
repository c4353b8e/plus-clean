namespace Plus.Communication.Packets.Outgoing.Messenger
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Game.Users;
    using Game.Users.Messenger;

    internal class BuddyListComposer : ServerPacket
    {
        public BuddyListComposer(ICollection<MessengerBuddy> friends, Habbo player, int pages, int page)
            : base(ServerPacketHeader.BuddyListMessageComposer)
        {
            WriteInteger(pages);// Pages
            WriteInteger(page);// Page

            WriteInteger(friends.Count);
            foreach (var Friend in friends.ToList())
            {
                var Relationship = player.Relationships.FirstOrDefault(x => x.Value.UserId == Convert.ToInt32(Friend.UserId)).Value;

                WriteInteger(Friend.Id);
                WriteString(Friend.mUsername);
                WriteInteger(1);//Gender.
                WriteBoolean(Friend.IsOnline);
                WriteBoolean(Friend.IsOnline && Friend.InRoom);
                WriteString(Friend.IsOnline ? Friend.mLook : string.Empty);
                WriteInteger(0); // category id
                WriteString(Friend.IsOnline ? Friend.mMotto : string.Empty);
                WriteString(string.Empty);//Alternative name?
                WriteString(string.Empty);
                WriteBoolean(true);
                WriteBoolean(false);
                WriteBoolean(false);//Pocket Habbo user.
                WriteShort(Relationship == null ? 0 : Relationship.Type);
            }
        }
    }
}
