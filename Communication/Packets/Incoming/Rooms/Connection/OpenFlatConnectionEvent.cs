namespace Plus.Communication.Packets.Incoming.Rooms.Connection
{
    using HabboHotel.GameClients;

    public class OpenFlatConnectionEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            if (session == null || session.GetHabbo() == null)
            {
                return;
            }

            var roomId = packet.PopInt();
            var password = packet.PopString();
        
            session.GetHabbo().PrepareRoom(roomId, password);
        }
    }
}