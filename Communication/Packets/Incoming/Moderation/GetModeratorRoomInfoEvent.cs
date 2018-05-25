namespace Plus.Communication.Packets.Incoming.Moderation
{
    using Game.Players;
    using Game.Rooms;
    using Outgoing.Moderation;

    internal class GetModeratorRoomInfoEvent : IPacketEvent
    {
        public void Parse(Player session, ClientPacket packet)
        {
            if (!session.GetHabbo().GetPermissions().HasRight("mod_tool"))
            {
                return;
            }

            var roomId = packet.PopInt();

            if (!RoomFactory.TryGetData(roomId, out var data))
            {
                return;
            }

            if (!Program.GameContext.GetRoomManager().TryGetRoom(roomId, out var room))
            {
                return;
            }

            session.SendPacket(new ModeratorRoomInfoComposer(data, room.GetRoomUserManager().GetRoomUserByHabbo(data.OwnerName) != null));
        }
    }
}
