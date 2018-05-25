namespace Plus.Communication.Packets.Incoming.Catalog
{
    using Game.Players;

    internal class GetCatalogModeEvent : IPacketEvent
    {
        public void Parse(Player session, ClientPacket packet)
        {
            // string mode = packet.PopString();
        }
    }
}
