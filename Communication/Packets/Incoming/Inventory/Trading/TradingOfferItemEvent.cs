namespace Plus.Communication.Packets.Incoming.Inventory.Trading
{
    using System.Linq;
    using HabboHotel.GameClients;
    using Outgoing.Inventory.Trading;

    internal class TradingOfferItemEvent : IPacketEvent
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

            var itemId = packet.PopInt();

            if (!roomUser.IsTrading)
            {
                session.SendPacket(new TradingClosedComposer(session.GetHabbo().Id));
                return;
            }

            if (!room.GetTrading().TryGetTrade(roomUser.TradeId, out var trade))
            {
                session.SendPacket(new TradingClosedComposer(session.GetHabbo().Id));
                return;
            }

            var item = session.GetHabbo().GetInventoryComponent().GetItem(itemId);
            if (item == null)
            {
                return;
            }

            if (!trade.CanChange)
            {
                return;
            }

            var tradeUser = trade.Users[0];

            if (tradeUser.RoomUser != roomUser)
            {
                tradeUser = trade.Users[1];
            }

            if (tradeUser.OfferedItems.ContainsKey(item.Id))
            {
                return;
            }

            trade.RemoveAccepted();

            if (tradeUser.OfferedItems.Count <= 499)
            {
                var totalLtDs = tradeUser.OfferedItems.Count(x => x.Value.LimitedNo > 0);

                if (totalLtDs < 9)
                {
                    tradeUser.OfferedItems.Add(item.Id, item);
                }
            }

            trade.SendPacket(new TradingUpdateComposer(trade));
        }
    }
}