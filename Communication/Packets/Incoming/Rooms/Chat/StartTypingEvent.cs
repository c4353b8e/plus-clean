namespace Plus.Communication.Packets.Incoming.Rooms.Chat
{
    using HabboHotel.GameClients;
    using Outgoing.Rooms.Chat;

    public class StartTypingEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            if (!session.GetHabbo().InRoom)
            {
                return;
            }

            var room = session.GetHabbo().CurrentRoom;
            if (room == null)
            {
                return;
            }

            var user = room.GetRoomUserManager().GetRoomUserByHabbo(session.GetHabbo().Username);
            if (user == null)
            {
                return;
            }

            session.GetHabbo().CurrentRoom.SendPacket(new UserTypingComposer(user.VirtualId, true));
        }
    }
}