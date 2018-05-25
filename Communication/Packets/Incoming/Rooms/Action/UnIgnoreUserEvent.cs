namespace Plus.Communication.Packets.Incoming.Rooms.Action
{
    using HabboHotel.GameClients;
    using HabboHotel.Users.Authenticator;
    using Outgoing.Rooms.Action;

    internal class UnIgnoreUserEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            if (!session.GetHabbo().InRoom)
            {
                return;
            }

            var room = session.GetHabbo().CurrentRoom;
            if (room == null)
            {
                return;
            }

            var username = packet.PopString();

            var player = HabboFactory.GetHabboByUsername(username);
            if (player == null)
            {
                return;
            }

            if (!session.GetHabbo().GetIgnores().TryGet(player.Id))
            {
                return;
            }

            if (session.GetHabbo().GetIgnores().TryRemove(player.Id))
            {
                using (var dbClient = Program.DatabaseManager.GetQueryReactor())
                {
                    dbClient.SetQuery("DELETE FROM `user_ignores` WHERE `user_id` = @uid AND `ignore_id` = @ignoreId");
                    dbClient.AddParameter("uid", session.GetHabbo().Id);
                    dbClient.AddParameter("ignoreId", player.Id);
                    dbClient.RunQuery();
                }

                session.SendPacket(new IgnoreStatusComposer(3, player.Username));
            }
        }
    }
}
