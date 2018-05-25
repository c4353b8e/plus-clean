namespace Plus.Communication.Packets.Incoming.Users
{
    using Game.Players;
    using Game.Users.Authenticator;
    using Outgoing.Users;

    internal class OpenPlayerProfileEvent : IPacketEvent
    {
        public void Parse(Player session, ClientPacket packet)
        {
            var userId = packet.PopInt();
            packet.PopBoolean(); //IsMe?

            var targetData = HabboFactory.GetHabboById(userId);
            if (targetData == null)
            {
                session.SendNotification("An error occured whilst finding that user's profile.");
                return;
            }
            
            var groups = Program.GameContext.GetGroupManager().GetGroupsForUser(targetData.Id);
            
            int friendCount;
            using (var dbClient = Program.DatabaseManager.GetQueryReactor())
            {
                dbClient.SetQuery("SELECT COUNT(0) FROM `messenger_friendships` WHERE (`user_one_id` = @userid OR `user_two_id` = @userid)");
                dbClient.AddParameter("userid", userId);
                friendCount = dbClient.GetInteger();
            }

            session.SendPacket(new ProfileInformationComposer(targetData, session, groups, friendCount));
        }
    }
}
