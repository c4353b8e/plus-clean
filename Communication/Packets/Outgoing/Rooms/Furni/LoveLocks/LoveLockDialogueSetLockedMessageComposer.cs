namespace Plus.Communication.Packets.Outgoing.Rooms.Furni.LoveLocks
{
    internal class LoveLockDialogueSetLockedMessageComposer : ServerPacket
    {
        public LoveLockDialogueSetLockedMessageComposer(int ItemId)
            : base(ServerPacketHeader.LoveLockDialogueSetLockedMessageComposer)
        {
            WriteInteger(ItemId);
        }
    }
}
