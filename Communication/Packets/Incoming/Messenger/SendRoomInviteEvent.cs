namespace Plus.Communication.Packets.Incoming.Messenger
{
    using System.Collections.Generic;
    using Game.Players;
    using Outgoing.Messenger;
    using Utilities;

    internal class SendRoomInviteEvent : IPacketEvent
    {
        public void Parse(Player session, ClientPacket packet)
        {
            if (session.GetHabbo().TimeMuted > 0)
            {
                session.SendNotification("Oops, you're currently muted - you cannot send room invitations.");
                return;
            }

            var amount = packet.PopInt();
            if (amount > 500)
            {
                return; // don't send at all
            }

            var targets = new List<int>();
            for (var i = 0; i < amount; i++)
            {
                var uid = packet.PopInt();
                if (i < 100) // limit to 100 people, keep looping until we fulfil the request though
                {
                    targets.Add(uid);
                }
            }

            var message = StringUtilities.Escape(packet.PopString());
            if (message.Length > 121)
            {
                message = message.Substring(0, 121);
            }

            foreach (var userId in targets)
            {
                if (!session.GetHabbo().GetMessenger().FriendshipExists(userId))
                {
                    continue;
                }

                var client = Program.GameContext.PlayerController.GetClientByUserId(userId);
                if (client == null || client.GetHabbo() == null || client.GetHabbo().AllowMessengerInvites || client.GetHabbo().AllowConsoleMessages == false)
                {
                    continue;
                }

                client.SendPacket(new RoomInviteComposer(session.GetHabbo().Id, message));
            }

            using (var dbClient = Program.DatabaseManager.GetQueryReactor())
            {
                dbClient.SetQuery("INSERT INTO `chatlogs_console_invitations` (`user_id`,`message`,`timestamp`) VALUES (@userId, @message, UNIX_TIMESTAMP())");
                dbClient.AddParameter("userId", session.GetHabbo().Id);
                dbClient.AddParameter("message", message);
                dbClient.RunQuery();
            }
        }
    }
}