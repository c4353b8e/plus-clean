﻿namespace Plus.Communication.Packets.Outgoing.Rooms.Avatar
{
    using Game.Rooms;

    public class SleepComposer : ServerPacket
    {
        public SleepComposer(RoomUser User, bool IsSleeping)
            : base(ServerPacketHeader.SleepMessageComposer)
        {
            WriteInteger(User.VirtualId);
            WriteBoolean(IsSleeping);
        }
    }
}