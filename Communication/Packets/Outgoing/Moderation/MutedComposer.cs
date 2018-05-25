namespace Plus.Communication.Packets.Outgoing.Moderation
{
    using System;

    internal class MutedComposer : ServerPacket
    {
        public MutedComposer(double TimeMuted)
            : base(ServerPacketHeader.MutedMessageComposer)
        {
            WriteInteger(Convert.ToInt32(TimeMuted));
        }
    }
}
