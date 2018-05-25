namespace Plus.Communication.Packets.Outgoing.Rooms.Engine
{
    internal class FurnitureAliasesComposer : ServerPacket
    {
        public FurnitureAliasesComposer()
            : base(ServerPacketHeader.FurnitureAliasesMessageComposer)
        {
            WriteInteger(0);          
        }
    }
}
