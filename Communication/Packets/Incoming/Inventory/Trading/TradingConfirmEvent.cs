﻿namespace Plus.Communication.Packets.Incoming.Inventory.Trading
{
    using HabboHotel.GameClients;
    using Outgoing.Inventory.Trading;

    internal class TradingConfirmEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            if (session == null || session.GetHabbo() == null || !session.GetHabbo().InRoom)
            {
                return;
            }

            var room = session.GetHabbo().CurrentRoom;
            if (room == null)
            {
                return;
            }

            var roomUser = room.GetRoomUserManager().GetRoomUserByHabbo(session.GetHabbo().Id);
            if (roomUser == null)
            {
                return;
            }

            if (!room.GetTrading().TryGetTrade(roomUser.TradeId, out var trade))
            {
                session.SendPacket(new TradingClosedComposer(session.GetHabbo().Id));
                return;
            }

            if (trade.CanChange)
            {
                return;
            }

            var user = trade.Users[0];
            if (user.RoomUser != roomUser)
            {
                user = trade.Users[1];
            }

            user.HasAccepted = true;
            trade.SendPacket(new TradingConfirmedComposer(session.GetHabbo().Id, true));

            if (trade.AllAccepted)
            {
                trade.Finish();
            }
        }
    }
}