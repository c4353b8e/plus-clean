namespace Plus.Communication.Packets.Outgoing.Groups
{
    using System;
    using System.Collections.Generic;
    using HabboHotel.Rooms;

    internal class GroupCreationWindowComposer : ServerPacket
    {
        public GroupCreationWindowComposer(ICollection<RoomData> rooms)
            : base(ServerPacketHeader.GroupCreationWindowMessageComposer)
        {
            WriteInteger(Convert.ToInt32(Program.SettingsManager.TryGetValue("catalog.group.purchase.cost")));//Price

            WriteInteger(rooms.Count);//Room count that the user has.
            foreach (var room in rooms)
            {
                WriteInteger(room.Id);//Room Id
                WriteString(room.Name);//Room Name
                WriteBoolean(false);//What?
            }

            WriteInteger(5);
            WriteInteger(5);
            WriteInteger(11);
            WriteInteger(4);

            WriteInteger(6);
            WriteInteger(11);
            WriteInteger(4);

            WriteInteger(0);
            WriteInteger(0);
            WriteInteger(0);

            WriteInteger(0);
            WriteInteger(0);
            WriteInteger(0);

            WriteInteger(0);
            WriteInteger(0);
            WriteInteger(0);
        }
    }
}