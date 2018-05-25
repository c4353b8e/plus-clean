namespace Plus.Communication.Packets.Incoming.Rooms.Settings
{
    using HabboHotel.GameClients;
    using HabboHotel.Navigator;

    internal class SaveEnforcedCategorySettingsEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            if (!Program.GameContext.GetRoomManager().TryGetRoom(packet.PopInt(), out var room))
            {
                return;
            }

            if (!room.CheckRights(session, true))
            {
                return;
            }

            var categoryId = packet.PopInt();
            var tradeSettings = packet.PopInt();

            if (tradeSettings < 0 || tradeSettings > 2)
            {
                tradeSettings = 0;
            }

            if (!Program.GameContext.GetNavigator().TryGetSearchResultList(categoryId, out var searchResultList))
            {
                categoryId = 36;
            }

            if (searchResultList.CategoryType != NavigatorCategoryType.Category || searchResultList.RequiredRank > session.GetHabbo().Rank)
            {
                categoryId = 36;
            }
        }
    }
}
