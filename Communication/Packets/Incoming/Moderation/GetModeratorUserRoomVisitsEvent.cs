namespace Plus.Communication.Packets.Incoming.Moderation
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using Game.Players;
    using Game.Rooms;
    using Outgoing.Moderation;

    internal class GetModeratorUserRoomVisitsEvent : IPacketEvent
    {
        public void Parse(Player session, ClientPacket packet)
        {
            if (!session.GetHabbo().GetPermissions().HasRight("mod_tool"))
            {
                return;
            }

            var userId = packet.PopInt();
            var target = Program.GameContext.PlayerController.GetClientByUserId(userId);
            if (target == null)
            {
                return;
            }

            var visits = new Dictionary<double, RoomData>();
            using (var dbClient = Program.DatabaseManager.GetQueryReactor())
            {
                dbClient.SetQuery("SELECT `room_id`, `entry_timestamp` FROM `user_roomvisits` WHERE `user_id` = @id ORDER BY `entry_timestamp` DESC LIMIT 50");
                dbClient.AddParameter("id", userId);
                var table = dbClient.GetTable();

                if (table != null)
                {
                    foreach (DataRow row in table.Rows)
                    {
                        if (!RoomFactory.TryGetData(Convert.ToInt32(row["room_id"]), out var data))
                        {
                            continue;
                        }

                        if (!visits.ContainsKey(Convert.ToDouble(row["entry_timestamp"])))
                        {
                            visits.Add(Convert.ToDouble(row["entry_timestamp"]), data);
                        }
                    }
                }
            }

            session.SendPacket(new ModeratorUserRoomVisitsComposer(target.GetHabbo(), visits));
        }
    }
}