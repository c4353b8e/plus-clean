namespace Plus.Communication.Packets.Incoming.Inventory.Badges
{
    using HabboHotel.GameClients;
    using HabboHotel.Quests;
    using Outgoing.Users;

    internal class SetActivatedBadgesEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            session.GetHabbo().GetBadgeComponent().ResetSlots();

            using (var dbClient = Program.DatabaseManager.GetQueryReactor())
            {
                dbClient.SetQuery("UPDATE `user_badges` SET `badge_slot` = '0' WHERE `user_id` = @userId");
                dbClient.AddParameter("userId", session.GetHabbo().Id);
                dbClient.RunQuery();
            }

            for (var i = 0; i < 5; i++)
            {
                var slot = packet.PopInt();
                var badge = packet.PopString();

                if (badge.Length == 0)
                {
                    continue;
                }

                if (!session.GetHabbo().GetBadgeComponent().HasBadge(badge) || slot < 1 || slot > 5)
                {
                    return;
                }

                session.GetHabbo().GetBadgeComponent().GetBadge(badge).Slot = slot;

                using (var dbClient = Program.DatabaseManager.GetQueryReactor())
                {
                    dbClient.SetQuery("UPDATE `user_badges` SET `badge_slot` = @slot WHERE `badge_id` = @badge AND `user_id` = @userId LIMIT 1");
                    dbClient.AddParameter("slot", slot);
                    dbClient.AddParameter("badge", badge);
                    dbClient.AddParameter("userId", session.GetHabbo().Id);
                    dbClient.RunQuery();
                }
            }

            Program.GameContext.GetQuestManager().ProgressUserQuest(session, QuestType.ProfileBadge);


            if (session.GetHabbo().InRoom && Program.GameContext.GetRoomManager().TryGetRoom(session.GetHabbo().CurrentRoomId, out var room))
            {
                room.SendPacket(new HabboUserBadgesComposer(session.GetHabbo()));
            }
            else
            {
                session.SendPacket(new HabboUserBadgesComposer(session.GetHabbo()));
            }
        }
    }
}
