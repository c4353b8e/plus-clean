namespace Plus.Communication.Packets.Incoming.Moderation
{
    using Game.Players;
    using Outgoing.Moderation;

    internal class CloseTicketEvent : IPacketEvent
    {
        public void Parse(Player session, ClientPacket packet)
        {
            if (session == null || session.GetHabbo() == null || !session.GetHabbo().GetPermissions().HasRight("mod_tool"))
            {
                return;
            }

            var result = packet.PopInt(); // 1 = useless, 2 = abusive, 3 = resolved
            packet.PopInt(); //junk
            var ticketId = packet.PopInt();
            
            if (!Program.GameContext.GetModerationManager().TryGetTicket(ticketId, out var ticket))
            {
                return;
            }

            if (ticket.Moderator.Id != session.GetHabbo().Id)
            {
                return;
            }

            var client = Program.GameContext.PlayerController.GetClientByUserId(ticket.Sender.Id);
            if (client != null)
            {
                client.SendPacket(new ModeratorSupportTicketResponseComposer(result));
            }

            if (result == 2)
            {
                using (var dbClient = Program.DatabaseManager.GetQueryReactor())
                {
                    dbClient.RunQuery("UPDATE `user_info` SET `cfhs_abusive` = `cfhs_abusive` + 1 WHERE `user_id` = '" + ticket.Sender.Id + "' LIMIT 1");
                }
            }

            ticket.Answered = true;
            Program.GameContext.PlayerController.SendPacket(new ModeratorSupportTicketComposer(session.GetHabbo().Id, ticket), "mod_tool");
        }
    }
}