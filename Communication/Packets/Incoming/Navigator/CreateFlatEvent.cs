namespace Plus.Communication.Packets.Incoming.Navigator
{
    using Game.Navigator;
    using Game.Players;
    using Game.Rooms;
    using Outgoing.Navigator;

    internal class CreateFlatEvent : IPacketEvent
    {
        public void Parse(Player session, ClientPacket packet)
        {
            if (session == null || session.GetHabbo() == null)
            {
                return;
            }

            var rooms = RoomFactory.GetRoomsDataByOwnerSortByName(session.GetHabbo().Id);
            if (rooms.Count >= 500)
            {
                session.SendPacket(new CanCreateRoomComposer(true, 500));
                return;
            }

            var name = Program.GameContext.GetChatManager().GetFilter().CheckMessage(packet.PopString());
            var description = Program.GameContext.GetChatManager().GetFilter().CheckMessage(packet.PopString());
            var modelName = packet.PopString();

            var category = packet.PopInt();
            var maxVisitors = packet.PopInt();//10 = min, 25 = max.
            var tradeSettings = packet.PopInt();//2 = All can trade, 1 = owner only, 0 = no trading.

            if (name.Length < 3)
            {
                return;
            }

            if (name.Length > 25)
            {
                return;
            }

            if (!Program.GameContext.GetRoomManager().TryGetModel(modelName, out var model))
            {
                return;
            }

            if (!Program.GameContext.GetNavigator().TryGetSearchResultList(category, out var searchResultList))
            {
                category = 36;
            }

            if (searchResultList.CategoryType != NavigatorCategoryType.Category || searchResultList.RequiredRank > session.GetHabbo().Rank)
            {
                category = 36;
            }

            if (maxVisitors < 10 || maxVisitors > 25)
            {
                maxVisitors = 10;
            }

            if (tradeSettings < 0 || tradeSettings > 2)
            {
                tradeSettings = 0;
            }

            var newRoom = Program.GameContext.GetRoomManager().CreateRoom(session, name, description, category, maxVisitors, tradeSettings, model);
            if (newRoom != null)
            {
                session.SendPacket(new FlatCreatedComposer(newRoom.Id, name));
            }

            if (session.GetHabbo() != null && session.GetHabbo().GetMessenger() != null)
            {
                session.GetHabbo().GetMessenger().OnStatusChanged(true);
            }
        }
    }
}
