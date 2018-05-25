namespace Plus.Communication.Packets.Incoming.Messenger
{
    using System.Linq;
    using Game.Players;

    internal class RemoveBuddyEvent : IPacketEvent
    {
        public void Parse(Player session, ClientPacket packet)
        {
            if (session == null || session.GetHabbo() == null || session.GetHabbo().GetMessenger() == null)
            {
                return;
            }

            var amount = packet.PopInt();
            if (amount > 100)
            {
                amount = 100;
            }
            else if (amount < 0)
            {
                return;
            }

            using (var dbClient = Program.DatabaseManager.GetQueryReactor())
            {
                for (var i = 0; i < amount; i++)
                {
                    var id = packet.PopInt();

                    if (session.GetHabbo().Relationships.Count(x => x.Value.UserId == id) > 0)
                    {
                        dbClient.SetQuery("DELETE FROM `user_relationships` WHERE `user_id` = @id AND `target` = @target OR `target` = @id AND `user_id` = @target");
                        dbClient.AddParameter("id", session.GetHabbo().Id);
                        dbClient.AddParameter("target", id);
                        dbClient.RunQuery();
                    }

                    if (session.GetHabbo().Relationships.ContainsKey(id))
                    {
                        session.GetHabbo().Relationships.Remove(id);
                    }

                    var target = Program.GameContext.PlayerController.GetClientByUserId(id);
                    if (target != null)
                    {
                        if (target.GetHabbo().Relationships.ContainsKey(session.GetHabbo().Id))
                        {
                            target.GetHabbo().Relationships.Remove(session.GetHabbo().Id);
                        }
                    }

                    session.GetHabbo().GetMessenger().DestroyFriendship(id);
                }
            }
        }
    }
}