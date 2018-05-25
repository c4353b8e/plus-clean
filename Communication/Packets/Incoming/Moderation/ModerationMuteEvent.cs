﻿namespace Plus.Communication.Packets.Incoming.Moderation
{
    using HabboHotel.Users.Authenticator;

    internal class ModerationMuteEvent : IPacketEvent
    {
        public void Parse(HabboHotel.GameClients.GameClient session, ClientPacket packet)
        {
            if (session?.GetHabbo() == null || !session.GetHabbo().GetPermissions().HasRight("mod_mute"))
            {
                return;
            }

            var userId = packet.PopInt();
            packet.PopString(); //message
            double length = packet.PopInt() * 60;
            packet.PopString(); //unk1
            packet.PopString(); //unk2

            var habbo = HabboFactory.GetHabboById(userId);
            if (habbo == null)
            {
                session.SendWhisper("An error occoured whilst finding that user in the database.");
                return;
            }

            if (habbo.GetPermissions().HasRight("mod_mute") && !session.GetHabbo().GetPermissions().HasRight("mod_mute_any"))
            {
                session.SendWhisper("Oops, you cannot mute that user.");
                return;
            }

            using (var dbClient = Program.DatabaseManager.GetQueryReactor())
            {
                dbClient.RunQuery("UPDATE `users` SET `time_muted` = '" + length + "' WHERE `id` = '" + habbo.Id + "' LIMIT 1");
            }

            if (habbo.GetClient() != null)
            {
                habbo.TimeMuted = length;
                habbo.GetClient().SendNotification("You have been muted by a moderator for " + length + " seconds!");
            }
        }
    }
}

