namespace Plus.Communication.Packets.Outgoing.Rooms.Furni.LoveLocks
{
    internal class LoveLockDialogueMessageComposer : ServerPacket
    {
        public LoveLockDialogueMessageComposer(int ItemId)
            : base(ServerPacketHeader.LoveLockDialogueMessageComposer)
        {
            WriteInteger(ItemId);
            WriteBoolean(true);
        }
    }
}
