namespace Plus.Communication.Packets.Incoming.Users
{
    using HabboHotel.GameClients;

    internal class SetUserFocusPreferenceEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            var focusPreference = packet.PopBoolean();

            session.GetHabbo().FocusPreference = focusPreference;
            using (var dbClient = Program.DatabaseManager.GetQueryReactor())
            {
                dbClient.SetQuery("UPDATE `users` SET `focus_preference` = @focusPreference WHERE `id` = '" + session.GetHabbo().Id + "' LIMIT 1");
                dbClient.AddParameter("focusPreference", focusPreference ? "1" : "0");
                dbClient.RunQuery();
            }
        }
    }
}
