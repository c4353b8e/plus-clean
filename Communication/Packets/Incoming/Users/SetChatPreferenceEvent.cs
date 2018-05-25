namespace Plus.Communication.Packets.Incoming.Users
{
    using HabboHotel.GameClients;

    internal class SetChatPreferenceEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            var preference = packet.PopBoolean();

            session.GetHabbo().ChatPreference = preference;
            using (var dbClient = Program.DatabaseManager.GetQueryReactor())
            {
                dbClient.SetQuery("UPDATE `users` SET `chat_preference` = @chatPreference WHERE `id` = '" + session.GetHabbo().Id + "' LIMIT 1");
                dbClient.AddParameter("chatPreference", preference ? "1" : "0");
                dbClient.RunQuery();
            }
        }
    }
}
