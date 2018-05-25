namespace Plus.Communication.Packets.Incoming.Rooms.Action
{
    using HabboHotel.GameClients;
    using Outgoing.Navigator;
    using Outgoing.Rooms.Session;

    internal class LetUserInEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            if (!Program.GameContext.GetRoomManager().TryGetRoom(session.GetHabbo().CurrentRoomId, out var room))
            {
                return;
            }

            if (!room.CheckRights(session))
            {
                return;
            }

            var name = packet.PopString();
            var accepted = packet.PopBoolean();

            var client = Program.GameContext.GetClientManager().GetClientByUsername(name);
            if (client == null)
            {
                return;
            }

            if (accepted)
            {
                client.GetHabbo().RoomAuthOk = true;
                client.SendPacket(new FlatAccessibleComposer(""));
                room.SendPacket(new FlatAccessibleComposer(client.GetHabbo().Username), true);
            }
            else
            {
                client.SendPacket(new FlatAccessDeniedComposer(""));
                room.SendPacket(new FlatAccessDeniedComposer(client.GetHabbo().Username), true);
            }
        }
    }
}
