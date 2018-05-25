namespace Plus.Communication.Packets.Incoming.Users
{
    using System.Collections.Generic;
    using HabboHotel.GameClients;
    using HabboHotel.Users.Authenticator;
    using Outgoing.Users;

    internal class GetIgnoredUsersEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            var ignoredUsers = new List<string>();

            foreach (var userId in new List<int>(session.GetHabbo().GetIgnores().IgnoredUserIds()))
            {
                var player = HabboFactory.GetHabboById(userId);
                if (player != null)
                {
                    if (!ignoredUsers.Contains(player.Username))
                    {
                        ignoredUsers.Add(player.Username);
                    }
                }
            }

            session.SendPacket(new IgnoredUsersComposer(ignoredUsers));
        }
    }
}