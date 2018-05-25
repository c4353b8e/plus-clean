namespace Plus.Communication.Packets.Incoming.Misc
{
    using Game.Players;

    internal class LatencyTestEvent : IPacketEvent
    {
        public void Parse(Player session, ClientPacket packet)
        {
            //Session.SendMessage(new LatencyTestComposer(Packet.PopInt()));
        }
    }
}
