namespace Plus.Communication.Packets.Outgoing.Rooms.Furni.LoveLocks
{
    internal class LoveLockDialogueCloseMessageComposer : ServerPacket
    {
        public LoveLockDialogueCloseMessageComposer(int ItemId)
            : base(ServerPacketHeader.LoveLockDialogueCloseMessageComposer)
        {
            WriteInteger(ItemId);
        }
    }
}
