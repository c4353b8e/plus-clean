namespace Plus.Communication.Packets.Incoming.Inventory.Trading
{
    using HabboHotel.GameClients;

    internal class TradingCancelConfirmEvent : IPacketEvent
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
                return;
            }

            trade.EndTrade(session.GetHabbo().Id);
        }
    }
}
