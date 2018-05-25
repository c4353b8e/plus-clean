namespace Plus.Communication.Packets.Incoming.Rooms.Settings
{
    using Game.Players;
    using Outgoing.Rooms.Settings;

    internal class GetRoomSettingsEvent : IPacketEvent
    {
        public void Parse(Player session, ClientPacket packet)
        {
            var roomId = packet.PopInt();

            if (!Program.GameContext.GetRoomManager().TryLoadRoom(roomId, out var room))
            {
                return;
            }

            if (!room.CheckRights(session, true))
            {
                return;
            }

            session.SendPacket(new RoomSettingsDataComposer(room));
        }
    }
}
