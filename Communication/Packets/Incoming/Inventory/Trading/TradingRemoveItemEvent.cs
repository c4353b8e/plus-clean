namespace Plus.Communication.Packets.Incoming.Inventory.Trading
{
    using HabboHotel.GameClients;
    using Outgoing.Inventory.Trading;

    internal class TradingRemoveItemEvent : IPacketEvent
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

            var user = trade.Users[0];
            if (user.RoomUser != roomUser)
            {
                user = trade.Users[1];
            }

            if (!user.OfferedItems.ContainsKey(item.Id))
            {
                return;
            }

            trade.RemoveAccepted();
            user.OfferedItems.Remove(item.Id);
            trade.SendPacket(new TradingUpdateComposer(trade));
        }
    }
}