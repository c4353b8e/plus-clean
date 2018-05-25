namespace Plus.Communication.Packets.Incoming.Users
{
    using System;
    using HabboHotel.GameClients;
    using HabboHotel.Users.Authenticator;
    using HabboHotel.Users.Relationships;
    using Outgoing.Messenger;
    using Outgoing.Moderation;

    internal class SetRelationshipEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            if (session == null || session.GetHabbo() == null || session.GetHabbo().GetMessenger() == null)
            {
                return;
            }

            var user = packet.PopInt();
            var type = packet.PopInt();

            if (!session.GetHabbo().GetMessenger().FriendshipExists(user))
            {
                session.SendPacket(new BroadcastMessageAlertComposer("Oops, you can only set a relationship where a friendship exists."));
                return;
            }

            if (type < 0 || type > 3)
            {
                session.SendPacket(new BroadcastMessageAlertComposer("Oops, you've chosen an invalid relationship type."));
                return;
            }

            if (session.GetHabbo().Relationships.Count > 2000)
            {
                session.SendPacket(new BroadcastMessageAlertComposer("Sorry, you're limited to a total of 2000 relationships."));
                return;
            }

            using (var dbClient = Program.DatabaseManager.GetQueryReactor())
            {
                if (type == 0)
                {
                    dbClient.SetQuery("SELECT `id` FROM `user_relationships` WHERE `user_id` = '" + session.GetHabbo().Id + "' AND `target` = @target LIMIT 1");
                    dbClient.AddParameter("target", user);

                    dbClient.SetQuery("DELETE FROM `user_relationships` WHERE `user_id` = '" + session.GetHabbo().Id + "' AND `target` = @target LIMIT 1");
                    dbClient.AddParameter("target", user);
                    dbClient.RunQuery();

                    if (session.GetHabbo().Relationships.ContainsKey(user))
                    {
                        session.GetHabbo().Relationships.Remove(user);
                    }
                }
                else
                {
                    dbClient.SetQuery("SELECT `id` FROM `user_relationships` WHERE `user_id` = '" + session.GetHabbo().Id + "' AND `target` = @target LIMIT 1");
                    dbClient.AddParameter("target", user);
                    var id = dbClient.GetInteger();

                    if (id > 0)
                    {
                        dbClient.SetQuery("DELETE FROM `user_relationships` WHERE `user_id` = '" + session.GetHabbo().Id + "' AND `target` = @target LIMIT 1");
                        dbClient.AddParameter("target", user);
                        dbClient.RunQuery();

                        if (session.GetHabbo().Relationships.ContainsKey(id))
                        {
                            session.GetHabbo().Relationships.Remove(id);
                        }
                    }

                    dbClient.SetQuery("INSERT INTO `user_relationships` (`user_id`,`target`,`type`) VALUES ('" + session.GetHabbo().Id + "', @target, @type)");
                    dbClient.AddParameter("target", user);
                    dbClient.AddParameter("type", type);
                    var newId = Convert.ToInt32(dbClient.InsertQuery());

                    if (!session.GetHabbo().Relationships.ContainsKey(user))
                    {
                        session.GetHabbo().Relationships.Add(user, new Relationship(newId, user, type));
                    }
                }

                var client = Program.GameContext.GetClientManager().GetClientByUserId(user);
                if (client != null)
                {
                    session.GetHabbo().GetMessenger().UpdateFriend(user, client, true);
                }
                else
                {
                    var habbo = HabboFactory.GetHabboById(user);
                    if (habbo != null)
                    {
                        if (session.GetHabbo().GetMessenger().TryGetFriend(user, out var buddy))
                        {
                            session.SendPacket(new FriendListUpdateComposer(session, buddy));
                        }
                    }
                }
            }
        }
    }
}