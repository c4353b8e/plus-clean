namespace Plus.Communication.Packets.Incoming.Rooms.Furni
{
    using HabboHotel.GameClients;
    using HabboHotel.Items;
    using Outgoing.Inventory.Furni;
    using Outgoing.Inventory.Purse;

    internal class CreditFurniRedeemEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            if (!session.GetHabbo().InRoom)
            {
                return;
            }

            if (!Program.GameContext.GetRoomManager().TryGetRoom(session.GetHabbo().CurrentRoomId, out var room))
            {
                return;
            }

            if (!room.CheckRights(session, true))
            {
                return;
            }

            if (Program.SettingsManager.TryGetValue("room.item.exchangeables.enabled") != "1")
            {
                session.SendNotification("The hotel managers have temporarilly disabled exchanging!");
                return;
            }

            var exchange = room.GetRoomItemHandler().GetItem(packet.PopInt());
            if (exchange == null)
            {
                return;
            }

            if (exchange.Data.InteractionType != InteractionType.EXCHANGE)
            {
                return;
            }


            var value = exchange.Data.BehaviourData;

            if (value > 0)
            {
                session.GetHabbo().Credits += value;
                session.SendPacket(new CreditBalanceComposer(session.GetHabbo().Credits));
            }

            using (var dbClient = Program.DatabaseManager.GetQueryReactor())
            {
                dbClient.SetQuery("DELETE FROM `items` WHERE `id` = @exchangeId LIMIT 1");
                dbClient.AddParameter("exchangeId", exchange.Id);
                dbClient.RunQuery();
            }

            session.SendPacket(new FurniListUpdateComposer());
            room.GetRoomItemHandler().RemoveFurniture(null, exchange.Id);
            session.GetHabbo().GetInventoryComponent().RemoveItem(exchange.Id);

        }
    }
}
