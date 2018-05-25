namespace Plus.Communication.Packets.Outgoing.Messenger
{
    using System.Collections.Generic;
    using System.Linq;
    using HabboHotel.Users.Messenger;

    internal class HabboSearchResultComposer : ServerPacket
    {
        public HabboSearchResultComposer(List<SearchResult> Friends, List<SearchResult> OtherUsers)
            : base(ServerPacketHeader.HabboSearchResultMessageComposer)
        {
            WriteInteger(Friends.Count);
            foreach (var Friend in Friends.ToList())
            {
                var Online = Program.GameContext.GetClientManager().GetClientByUserId(Friend.UserId) != null;

                WriteInteger(Friend.UserId);
               WriteString(Friend.Username);
               WriteString(Friend.Motto);
                WriteBoolean(Online);
                WriteBoolean(false);
               WriteString(string.Empty);
                WriteInteger(0);
               WriteString(Online ? Friend.Figure : "");
               WriteString(Friend.LastOnline);
            }

            WriteInteger(OtherUsers.Count);
            foreach (var OtherUser in OtherUsers.ToList())
            {
                var Online = Program.GameContext.GetClientManager().GetClientByUserId(OtherUser.UserId) != null;

                WriteInteger(OtherUser.UserId);
               WriteString(OtherUser.Username);
               WriteString(OtherUser.Motto);
                WriteBoolean(Online);
                WriteBoolean(false);
               WriteString(string.Empty);
                WriteInteger(0);
               WriteString(Online ? OtherUser.Figure : "");
               WriteString(OtherUser.LastOnline);
            }
        }
    }
}
