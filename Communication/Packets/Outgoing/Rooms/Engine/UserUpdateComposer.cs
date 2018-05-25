namespace Plus.Communication.Packets.Outgoing.Rooms.Engine
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using HabboHotel.Rooms;

    internal class UserUpdateComposer : ServerPacket
    {
        public UserUpdateComposer(ICollection<RoomUser> users)
            : base(ServerPacketHeader.UserUpdateMessageComposer)
        {
            WriteInteger(users.Count);
            foreach (var user in users.ToList())
            {
                WriteInteger(user.VirtualId);
                WriteInteger(user.X);
                WriteInteger(user.Y);
                WriteString(user.Z.ToString("0.00"));
                WriteInteger(user.RotHead);
                WriteInteger(user.RotBody);

                var StatusComposer = new StringBuilder();
                StatusComposer.Append("/");

                foreach (var Status in user.Statusses.ToList())
                {
                    StatusComposer.Append(Status.Key);

                    if (!string.IsNullOrEmpty(Status.Value))
                    {
                        StatusComposer.Append(" ");
                        StatusComposer.Append(Status.Value);
                    }

                    StatusComposer.Append("/");
                }

                StatusComposer.Append("/");
                WriteString(StatusComposer.ToString());
            }
        }
    }
}