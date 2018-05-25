namespace Plus.Communication.Packets.Outgoing.Catalog
{
    internal class CatalogUpdatedComposer : ServerPacket
    {
        public CatalogUpdatedComposer()
            : base(ServerPacketHeader.CatalogUpdatedMessageComposer)
        {
            WriteBoolean(false);
        }
    }
}
