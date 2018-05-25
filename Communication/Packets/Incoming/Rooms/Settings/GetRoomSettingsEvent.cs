namespace Plus.Communication.Packets.Incoming.Rooms.Settings
{
    using HabboHotel.GameClients;
    using Outgoing.Rooms.Settings;

    internal class GetRoomSettingsEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
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
