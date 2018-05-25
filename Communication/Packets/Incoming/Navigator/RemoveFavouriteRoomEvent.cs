namespace Plus.Communication.Packets.Incoming.Navigator
{
    using Game.Players;
    using Outgoing.Navigator;

    public class RemoveFavouriteRoomEvent : IPacketEvent
    {
        public void Parse(Player session, ClientPacket packet)
        {
            var id = packet.PopInt();

            session.GetHabbo().FavoriteRooms.Remove(id);
            session.SendPacket(new UpdateFavouriteRoomComposer(id, false));

            using (var dbClient = Program.DatabaseManager.GetQueryReactor())
            {
                dbClient.RunQuery("DELETE FROM user_favorites WHERE user_id = " + session.GetHabbo().Id + " AND room_id = " + id + " LIMIT 1");
            }
        }
    }
}