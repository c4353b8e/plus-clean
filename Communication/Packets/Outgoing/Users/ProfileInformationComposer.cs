namespace Plus.Communication.Packets.Outgoing.Users
{
    using System;
    using System.Collections.Generic;
    using Game.Groups;
    using Game.Players;
    using Game.Users;
    using Utilities;

    internal class ProfileInformationComposer : ServerPacket
    {
        public ProfileInformationComposer(Habbo habbo, Player session, List<Group> groups, int friendCount)
            : base(ServerPacketHeader.ProfileInformationMessageComposer)
        {
            var origin = new DateTime(1970, 1, 1, 0, 0, 0, 0).AddSeconds(habbo.AccountCreated);

            WriteInteger(habbo.Id);
            WriteString(habbo.Username);
            WriteString(habbo.Look);
            WriteString(habbo.Motto);
            WriteString(origin.ToString("dd/MM/yyyy"));
            WriteInteger(habbo.GetStats().AchievementPoints);
            WriteInteger(friendCount); // Friend Count
            WriteBoolean(habbo.Id != session.GetHabbo().Id && session.GetHabbo().GetMessenger().FriendshipExists(habbo.Id)); //  Is friend
            WriteBoolean(habbo.Id != session.GetHabbo().Id && !session.GetHabbo().GetMessenger().FriendshipExists(habbo.Id) && session.GetHabbo().GetMessenger().RequestExists(habbo.Id)); // Sent friend request
            WriteBoolean(Program.GameContext.PlayerController.GetClientByUserId(habbo.Id) != null);

            WriteInteger(groups.Count);
            foreach (var group in groups)
            {
                WriteInteger(group.Id);
                WriteString(group.Name);
                WriteString(group.Badge);
                WriteString(Program.GameContext.GetGroupManager().GetColourCode(group.Colour1, true));
                WriteString(Program.GameContext.GetGroupManager().GetColourCode(group.Colour2, false));
                WriteBoolean(habbo.GetStats().FavouriteGroupId == group.Id); // todo favs
                WriteInteger(0);//what the fuck
                WriteBoolean(group != null ? group.ForumEnabled : true);//HabboTalk
            }

            WriteInteger(Convert.ToInt32(UnixUtilities.GetNow() - habbo.LastOnline)); // Last online
            WriteBoolean(true); // Show the profile
        }
    }
}