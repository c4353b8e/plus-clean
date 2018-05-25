namespace Plus.Communication.Packets.Incoming.Rooms.Chat
{
    using Game.Players;
    using Outgoing.Rooms.Chat;

    public class CancelTypingEvent : IPacketEvent
    {
        public void Parse(Player session, ClientPacket packet)
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

            session.GetHabbo().CurrentRoom.SendPacket(new UserTypingComposer(user.VirtualId, false));
        }
    }
}