﻿namespace Plus.Communication.Packets.Incoming.Users
{
    using HabboHotel.GameClients;
    using HabboHotel.Users.Authenticator;
    using Outgoing.Users;

    internal class GetSelectedBadgesEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            var userId = packet.PopInt();
            var habbo = HabboFactory.GetHabboById(userId);
            if (habbo == null)
            {
                return;
            }

            session.SendPacket(new HabboUserBadgesComposer(habbo));
        }
    }
}