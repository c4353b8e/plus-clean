namespace Plus.Communication.Packets.Outgoing.Rooms.Engine
{
    using Game.Items;

    internal class ObjectRemoveComposer : ServerPacket
    {
        public ObjectRemoveComposer(Item Item, int UserId)
            : base(ServerPacketHeader.ObjectRemoveMessageComposer)
        {
           WriteString(Item.Id.ToString());
            WriteBoolean(false);
            WriteInteger(UserId);
            WriteInteger(0);
        }
    }
}