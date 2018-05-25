namespace Plus.Communication.Packets.Incoming.Moderation
{
    using Game.Players;
    using Outgoing.Moderation;

    internal class PickTicketEvent : IPacketEvent
    {
        public void Parse(Player session, ClientPacket packet)
        {
            if (session == null || session.GetHabbo() == null || !session.GetHabbo().GetPermissions().HasRight("mod_tool"))
            {
                return;
            }

            packet.PopInt();//Junk
            var ticketId = packet.PopInt();

            if (!Program.GameContext.GetModerationManager().TryGetTicket(ticketId, out var ticket))
            {
                return;
            }

            ticket.Moderator = session.GetHabbo();
            Program.GameContext.PlayerController.SendPacket(new ModeratorSupportTicketComposer(session.GetHabbo().Id, ticket), "mod_tool");
        }
    }
}
