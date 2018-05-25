namespace Plus.Communication.Packets.Incoming.Inventory.Trading
{
    using HabboHotel.GameClients;
    using Outgoing.Inventory.Trading;
    using Utilities;

    internal class InitTradeEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            var userId = packet.PopInt();

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

            var targetUser = room.GetRoomUserManager().GetRoomUserByVirtualId(userId);
            if (targetUser == null)
            {
                return;
            }

            if (session.GetHabbo().TradingLockExpiry > 0)
            {
                if (session.GetHabbo().TradingLockExpiry > UnixTimestamp.GetNow())
                {
                    session.SendNotification("You're currently banned from trading.");
                    return;
                }

                session.GetHabbo().TradingLockExpiry = 0;
                session.SendNotification("Your trading ban has now expired.");

                using (var dbClient = Program.DatabaseManager.GetQueryReactor())
                {
                    dbClient.RunQuery("UPDATE `user_info` SET `trading_locked` = '0' WHERE `id` = '" + session.GetHabbo().Id + "' LIMIT 1");
                }
            }

            if (!session.GetHabbo().GetPermissions().HasRight("room_trade_override"))
            {
                if (room.TradeSettings == 0)
                {
                    session.SendPacket(new TradingErrorComposer(6, targetUser.GetUsername()));
                    return;
                }

                if (room.TradeSettings == 1 && room.OwnerId != session.GetHabbo().Id)
                {
                    session.SendPacket(new TradingErrorComposer(6, targetUser.GetUsername()));
                    return;
                }
            }

            if (roomUser.IsTrading && roomUser.TradePartner != targetUser.UserId)
            {
                session.SendPacket(new TradingErrorComposer(7, targetUser.GetUsername()));
                return;
            }

            if (targetUser.IsTrading && targetUser.TradePartner != roomUser.UserId)
            {
                session.SendPacket(new TradingErrorComposer(8, targetUser.GetUsername()));
                return;
            }

            if (!targetUser.GetClient().GetHabbo().AllowTradingRequests)
            {
                session.SendPacket(new TradingErrorComposer(4, targetUser.GetUsername()));
                return;
            }

            if (targetUser.GetClient().GetHabbo().TradingLockExpiry > 0)
            {
                session.SendPacket(new TradingErrorComposer(4, targetUser.GetUsername()));
                return;
            }

            if (!room.GetTrading().StartTrade(roomUser, targetUser, out var trade))
            {
                session.SendNotification("An error occured trying to start this trade");
                return;
            }

            if (targetUser.HasStatus("trd"))
            {
                targetUser.RemoveStatus("trd");
            }

            if (roomUser.HasStatus("trd"))
            {
                roomUser.RemoveStatus("trd");
            }

            targetUser.SetStatus("trd");
            targetUser.UpdateNeeded = true;
            roomUser.SetStatus("trd");
            roomUser.UpdateNeeded = true;

            trade.SendPacket(new TradingStartComposer(roomUser.UserId, targetUser.UserId));
        }
    }
}