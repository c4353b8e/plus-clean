namespace Plus.Communication.Packets.Incoming.Moderation
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using Game.Players;
    using Game.Rooms.Chat.Logs;
    using Game.Users.Authenticator;
    using Outgoing.Moderation;

    internal class GetModeratorRoomChatlogEvent : IPacketEvent
    {
        public void Parse(Player session, ClientPacket packet)
        {
            if (session == null || session.GetHabbo() == null)
            {
                return;
            }

            if (!session.GetHabbo().GetPermissions().HasRight("mod_tool"))
            {
                return;
            }

            packet.PopInt(); //junk
            var roomId = packet.PopInt();

            if (!Program.GameContext.GetRoomManager().TryGetRoom(roomId, out var room))
            {
                return;
            }

            Program.GameContext.GetChatManager().GetLogs().FlushAndSave();

            var chats = new List<ChatlogEntry>();

            using (var dbClient = Program.DatabaseManager.GetQueryReactor())
            {
                dbClient.SetQuery("SELECT * FROM `chatlogs` WHERE `room_id` = @id ORDER BY `id` DESC LIMIT 100");
                dbClient.AddParameter("id", roomId);
                var data = dbClient.GetTable();

                if (data != null)
                {
                    foreach (DataRow row in data.Rows)
                    {
                        var habbo = HabboFactory.GetHabboById(Convert.ToInt32(row["user_id"]));

                        if (habbo != null)
                        {
                            chats.Add(new ChatlogEntry(Convert.ToInt32(row["user_id"]), roomId, Convert.ToString(row["message"]), Convert.ToDouble(row["timestamp"]), habbo));
                        }
                    }
                }
            }

            session.SendPacket(new ModeratorRoomChatlogComposer(room, chats));
        }
    }
}