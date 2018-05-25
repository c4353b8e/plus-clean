namespace Plus.Communication.Packets.Incoming.Inventory.Trading
{
    using System.Linq;
    using Game.Players;
    using Outgoing.Inventory.Trading;

    internal class TradingOfferItemsEvent : IPacketEvent
    {
        public void Parse(Player session, ClientPacket packet)
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

            var amount = packet.PopInt();
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

            var allItems = session.GetHabbo().GetInventoryComponent().GetItems.Where(x => x.Data.Id == item.Data.Id).Take(amount).ToList();
            foreach (var I in allItems)
            {
                if (tradeUser.OfferedItems.ContainsKey(I.Id))
                {
                    return;
                }

                trade.RemoveAccepted();

                tradeUser.OfferedItems.Add(I.Id, I);
            }

            trade.SendPacket(new TradingUpdateComposer(trade));
        }
    }
}
