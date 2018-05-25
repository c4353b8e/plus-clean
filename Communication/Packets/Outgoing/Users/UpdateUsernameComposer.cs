﻿namespace Plus.Communication.Packets.Outgoing.Users
{
    internal class UpdateUsernameComposer : ServerPacket
    {
        public UpdateUsernameComposer(string username)
            : base(ServerPacketHeader.UpdateUsernameMessageComposer)
        {
            WriteInteger(0);
            WriteString(username);
            WriteInteger(0);
        }
    }
}