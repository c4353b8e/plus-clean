namespace Plus.Communication.Packets.Incoming.Handshake
{
    using HabboHotel.GameClients;

    internal class PingEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            session.PingCount = 0;
        }
    }
}
