﻿namespace Plus.Communication.Packets.Incoming.Users
{
    using Game.Players;

    internal class SetUserFocusPreferenceEvent : IPacketEvent
    {
        public void Parse(Player session, ClientPacket packet)
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
