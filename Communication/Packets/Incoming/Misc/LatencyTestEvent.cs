namespace Plus.Communication.Packets.Incoming.Misc
{
    using HabboHotel.GameClients;

    internal class LatencyTestEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            //Session.SendMessage(new LatencyTestComposer(Packet.PopInt()));
        }
    }
}
