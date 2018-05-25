namespace Plus.Communication.Packets.Outgoing.Rooms.Engine
{
    using Game.Items;

    internal class ItemRemoveComposer : ServerPacket
    {
        public ItemRemoveComposer(Item Item, int UserId)
            : base(ServerPacketHeader.ItemRemoveMessageComposer)
        {
            WriteString(Item.Id.ToString());
            WriteBoolean(false);
            WriteInteger(UserId);
        }
    }
}
