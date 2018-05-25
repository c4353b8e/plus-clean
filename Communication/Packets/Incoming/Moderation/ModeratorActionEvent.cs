namespace Plus.Communication.Packets.Incoming.Moderation
{
    using Outgoing.Moderation;

    internal class ModeratorActionEvent : IPacketEvent
    {
        public void Parse(HabboHotel.GameClients.GameClient session, ClientPacket packet)
        {
            if (!session.GetHabbo().GetPermissions().HasRight("mod_caution"))
            {
                return;
            }

            if (!session.GetHabbo().InRoom)
            {
                return;
            }

            var currentRoom = session.GetHabbo().CurrentRoom;
            if (currentRoom == null)
            {
                return;
            }

            var alertMode = packet.PopInt(); 
            var alertMessage = packet.PopString();
            var isCaution = alertMode != 3;

            alertMessage = isCaution ? "Caution from Moderator:\n\n" + alertMessage : "Message from Moderator:\n\n" + alertMessage;
            session.GetHabbo().CurrentRoom.SendPacket(new BroadcastMessageAlertComposer(alertMessage));
        }
    }
}
