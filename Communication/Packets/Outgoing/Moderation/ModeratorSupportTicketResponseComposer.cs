namespace Plus.Communication.Packets.Outgoing.Moderation
{
    internal class ModeratorSupportTicketResponseComposer : ServerPacket
    {
        public ModeratorSupportTicketResponseComposer(int result)
            : base(ServerPacketHeader.ModeratorSupportTicketResponseMessageComposer)
        {
            WriteInteger(result);
            WriteString("");
        }
    }
}