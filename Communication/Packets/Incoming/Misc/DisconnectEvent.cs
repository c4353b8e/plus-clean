namespace Plus.Communication.Packets.Incoming.Misc
{
    using Game.Players;

    internal class DisconnectEvent : IPacketEvent
    {
        public void Parse(Player session, ClientPacket packet)
        {
            session.Disconnect();
        }
    }
}
