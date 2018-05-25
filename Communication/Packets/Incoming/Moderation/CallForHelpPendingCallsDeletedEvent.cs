namespace Plus.Communication.Packets.Incoming.Moderation
{
    using HabboHotel.GameClients;
    using Outgoing.Moderation;

    internal class CallForHelpPendingCallsDeletedEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            if (session == null || session.GetHabbo() == null)
            {
                return;
            }

            if (Program.GameContext.GetModerationManager().UserHasTickets(session.GetHabbo().Id))
            {
                var pendingTicket = Program.GameContext.GetModerationManager().GetTicketBySenderId(session.GetHabbo().Id);
                if (pendingTicket != null)
                {
                    pendingTicket.Answered = true;
                    Program.GameContext.GetClientManager().SendPacket(new ModeratorSupportTicketComposer(session.GetHabbo().Id, pendingTicket), "mod_tool");
                }
            }
        }
    }
}
