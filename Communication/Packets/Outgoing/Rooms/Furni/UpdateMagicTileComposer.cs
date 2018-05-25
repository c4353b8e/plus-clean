namespace Plus.Communication.Packets.Outgoing.Rooms.Furni
{
    using System;

    internal class UpdateMagicTileComposer : ServerPacket
    {
        public UpdateMagicTileComposer(int ItemId, int Decimal)
            : base(ServerPacketHeader.UpdateMagicTileMessageComposer)
        {
            WriteInteger(Convert.ToInt32(ItemId));
            WriteInteger(Decimal);
        }
    }
}
