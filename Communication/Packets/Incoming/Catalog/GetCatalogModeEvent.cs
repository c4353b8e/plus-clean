namespace Plus.Communication.Packets.Incoming.Catalog
{
    using HabboHotel.GameClients;

    internal class GetCatalogModeEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            // string mode = packet.PopString();
        }
    }
}
