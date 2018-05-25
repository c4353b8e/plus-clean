namespace Plus.Communication.Packets.Incoming.Moderation
{
    using Game.Players;
    using Game.Rooms;
    using Outgoing.Moderation;

    internal class GetModeratorTicketChatlogsEvent : IPacketEvent
    {
        public void Parse(Player session, ClientPacket packet)
        {
            if (session == null || session.GetHabbo() == null || !session.GetHabbo().GetPermissions().HasRight("mod_tickets"))
            {
                return;
            }

            var ticketId = packet.PopInt();

            if (!Program.GameContext.GetModerationManager().TryGetTicket(ticketId, out var ticket) || ticket.Room == null)
            {
                return;
            }

            if (!RoomFactory.TryGetData(ticket.Room.Id, out var data))
            {
                return;
            }

            session.SendPacket(new ModeratorTicketChatlogComposer(ticket, data, ticket.Timestamp));
        }
    }
}
