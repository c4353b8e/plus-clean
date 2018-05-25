namespace Plus.Communication.Packets.Incoming.Handshake
{
    using Game.Players;

    internal class PingEvent : IPacketEvent
    {
        public void Parse(Player session, ClientPacket packet)
        {
            session.PingCount = 0;
        }
    }
}
