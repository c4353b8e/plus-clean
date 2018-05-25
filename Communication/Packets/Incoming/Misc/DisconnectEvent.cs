namespace Plus.Communication.Packets.Incoming.Misc
{
    using HabboHotel.GameClients;

    internal class DisconnectEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            session.Disconnect();
        }
    }
}
