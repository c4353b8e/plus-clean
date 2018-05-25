namespace Plus.Communication.Packets.Incoming.Moderation
{
    using Game.Players;

    internal class ModerationKickEvent : IPacketEvent
    {
        public void Parse(Player session, ClientPacket packet)
        {
            if (session == null || session.GetHabbo() == null || !session.GetHabbo().GetPermissions().HasRight("mod_kick"))
            {
                return;
            }

            var userId = packet.PopInt();
            packet.PopString(); //message

            var client = Program.GameContext.PlayerController.GetClientByUserId(userId);
            if (client == null || client.GetHabbo() == null || client.GetHabbo().CurrentRoomId < 1 || client.GetHabbo().Id == session.GetHabbo().Id)
            {
                return;
            }

            if (client.GetHabbo().Rank >= session.GetHabbo().Rank)
            {
                session.SendNotification(Program.LanguageManager.TryGetValue("moderation.kick.disallowed"));
                return;
            }

            if (!Program.GameContext.GetRoomManager().TryGetRoom(session.GetHabbo().CurrentRoomId, out var room))
            {
                return;
            }

            room.GetRoomUserManager().RemoveUserFromRoom(client, true);
        }
    }
}
