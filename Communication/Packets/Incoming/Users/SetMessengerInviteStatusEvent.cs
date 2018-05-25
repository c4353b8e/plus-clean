namespace Plus.Communication.Packets.Incoming.Users
{
    using Game.Players;

    internal class SetMessengerInviteStatusEvent : IPacketEvent
    {
        public void Parse(Player session, ClientPacket packet)
        {
            var status = packet.PopBoolean();

            session.GetHabbo().AllowMessengerInvites = status;
            using (var dbClient = Program.DatabaseManager.GetQueryReactor())
            {
                dbClient.SetQuery("UPDATE `users` SET `ignore_invites` = @MessengerInvites WHERE `id` = '" + session.GetHabbo().Id + "' LIMIT 1");
                dbClient.AddParameter("MessengerInvites", status ? "1" : "0");
                dbClient.RunQuery();
            }
        }
    }
}
