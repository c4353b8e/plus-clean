namespace Plus.Communication.Packets.Incoming.Rooms.Action
{
    using Game.Players;
    using Outgoing.Navigator;

    internal class GiveRoomScoreEvent : IPacketEvent
    {
        public void Parse(Player session, ClientPacket packet)
        {
            if (!session.GetHabbo().InRoom)
            {
                return;
            }

            if (!Program.GameContext.GetRoomManager().TryGetRoom(session.GetHabbo().CurrentRoomId, out var room))
            {
                return;
            }

            if (session.GetHabbo().RatedRooms.Contains(room.RoomId) || room.CheckRights(session, true))
            {
                return;
            }

            var rating = packet.PopInt();
            switch (rating)
            {
                case -1:
                    room.Score--;
                    break;
                case 1:
                    room.Score++;
                    break;
                default:
                    return;
            }
          
            using (var dbClient = Program.DatabaseManager.GetQueryReactor())
            {
                dbClient.RunQuery("UPDATE rooms SET score = '" + room.Score + "' WHERE id = '" + room.RoomId + "' LIMIT 1");
            }

            session.GetHabbo().RatedRooms.Add(room.RoomId);        
            session.SendPacket(new RoomRatingComposer(room.Score, !(session.GetHabbo().RatedRooms.Contains(room.RoomId) || room.CheckRights(session, true))));
        }
    }
}
