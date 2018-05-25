﻿namespace Plus.Communication.Packets.Incoming.Navigator
{
    using HabboHotel.Rooms;
    using Outgoing.Navigator;

    internal class GetGuestRoomEvent : IPacketEvent
    {
        public void Parse(HabboHotel.GameClients.GameClient session, ClientPacket packet)
        {
            var roomId = packet.PopInt();

            if (!RoomFactory.TryGetData(roomId, out var data))
            {
                return;
            }

            var enter = packet.PopInt() == 1;
            var forward = packet.PopInt() == 1;

            session.SendPacket(new GetGuestRoomResultComposer(session, data, enter, forward));
        }
    }
}
