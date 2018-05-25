namespace Plus.Communication.Packets.Incoming.Moderation
{
    using HabboHotel.GameClients;
    using Outgoing.Moderation;

    internal class ReleaseTicketEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            if (session == null || session.GetHabbo() == null || !session.GetHabbo().GetPermissions().HasRight("mod_tool"))
            {
                return;
            }

            var amount = packet.PopInt();

            for (var i = 0; i < amount; i++)
            {
                if (!Program.GameContext.GetModerationManager().TryGetTicket(packet.PopInt(), out var ticket))
                {
                    continue;
                }

                ticket.Moderator = null;
                Program.GameContext.GetClientManager().SendPacket(new ModeratorSupportTicketComposer(session.GetHabbo().Id, ticket), "mod_tool");
            }
        }
    }
}