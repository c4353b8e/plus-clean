namespace Plus.Communication.Packets.Outgoing.Moderation
{
    using System.Collections.Generic;
    using Game.Rooms;
    using Game.Rooms.Chat.Logs;
    using Utilities;

    internal class ModeratorRoomChatlogComposer : ServerPacket
    {
        public ModeratorRoomChatlogComposer(Room room, ICollection<ChatlogEntry> chats)
            : base(ServerPacketHeader.ModeratorRoomChatlogMessageComposer)
        {
            WriteByte(1);
            WriteShort(2);//Count
            WriteString("roomName");
            WriteByte(2);
            WriteString(room.Name);
            WriteString("roomId");
            WriteByte(1);
            WriteInteger(room.Id);

            WriteShort(chats.Count);
            foreach (var Entry in chats)
            {
                var Username = "Unknown";
                if (Entry.PlayerNullable() != null)
                {
                    Username = Entry.PlayerNullable().Username;
                }

                WriteString(UnixUtilities.FromUnixTimestamp(Entry.Timestamp).ToShortTimeString()); // time?
                WriteInteger(Entry.PlayerId); // User Id
                WriteString(Username); // Username
                WriteString(!string.IsNullOrEmpty(Entry.Message) ? Entry.Message : "** user sent a blank message **"); // Message        
                WriteBoolean(false); //TODO, AI's?
            }
        }
    }
}
