namespace Plus.Communication.Packets.Outgoing.Moderation
{
    using System.Collections.Generic;
    using HabboHotel.Rooms;
    using HabboHotel.Rooms.Chat.Logs;
    using HabboHotel.Users;
    using Utilities;

    internal class ModeratorUserChatlogComposer : ServerPacket
    {
        public ModeratorUserChatlogComposer(Habbo habbo, List<KeyValuePair<RoomData, List<ChatlogEntry>>> chatlogs)
            : base(ServerPacketHeader.ModeratorUserChatlogMessageComposer)
        {
            WriteInteger(habbo.Id);
            WriteString(habbo.Username);

            WriteInteger(chatlogs.Count); // Room Visits Count
            foreach (var Chatlog in chatlogs)
            {
                WriteByte(1);
                WriteShort(2);//Count
                WriteString("roomName");
                WriteByte(2);
                WriteString(Chatlog.Key.Name); // room name
                WriteString("roomId");
                WriteByte(1);
                WriteInteger(Chatlog.Key.Id);

                WriteShort(Chatlog.Value.Count); // Chatlogs Count
                foreach (var Entry in Chatlog.Value)
                {
                    var Username = "NOT FOUND";
                    if (Entry.PlayerNullable() != null)
                    {
                        Username = Entry.PlayerNullable().Username;
                    }

                    WriteString(UnixTimestamp.FromUnixTimestamp(Entry.Timestamp).ToShortTimeString());
                    WriteInteger(Entry.PlayerId); // UserId of message
                    WriteString(Username); // Username of message
                    WriteString(!string.IsNullOrEmpty(Entry.Message) ? Entry.Message : "** user sent a blank message **"); // Message        
                    WriteBoolean(habbo.Id == Entry.PlayerId);
                }
            }
        }
    }
}