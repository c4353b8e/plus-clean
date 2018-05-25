namespace Plus.Communication.Packets.Incoming.Moderation
{
    using System.Linq;
    using Game.Players;
    using Game.Rooms;
    using Outgoing.Navigator;
    using Outgoing.Rooms.Settings;

    internal class ModerateRoomEvent : IPacketEvent
    {
        public void Parse(Player session, ClientPacket packet)
        {
            if (!session.GetHabbo().GetPermissions().HasRight("mod_tool"))
            {
                return;
            }

            if (!Program.GameContext.GetRoomManager().TryGetRoom(packet.PopInt(), out var room))
            {
                return;
            }

            var setLock = packet.PopInt() == 1;
            var setName = packet.PopInt() == 1;
            var kickAll = packet.PopInt() == 1;

            if (setName)
            {
                room.Name = "Inappropriate to Hotel Management";
                room.Description = "Inappropriate to Hotel Management";
            }

            if (setLock)
            {
                room.Access = RoomAccess.Doorbell;
            }

            if (room.Tags.Count > 0)
            {
                room.ClearTags();
            }

            if (room.HasActivePromotion)
            {
                room.EndPromotion();
            }

            using (var dbClient = Program.DatabaseManager.GetQueryReactor())
            {
                if (setName && setLock)
                {
                    dbClient.RunQuery("UPDATE `rooms` SET `caption` = 'Inappropriate to Hotel Management', `description` = 'Inappropriate to Hotel Management', `tags` = '', `state` = '1' WHERE `id` = '" + room.RoomId + "' LIMIT 1");
                }
                else if (setName)
                {
                    dbClient.RunQuery("UPDATE `rooms` SET `caption` = 'Inappropriate to Hotel Management', `description` = 'Inappropriate to Hotel Management', `tags` = '' WHERE `id` = '" + room.RoomId + "' LIMIT 1");
                }
                else if (setLock)
                {
                    dbClient.RunQuery("UPDATE `rooms` SET `state` = '1', `tags` = '' WHERE `id` = '" + room.RoomId + "' LIMIT 1");
                }
            }

            room.SendPacket(new RoomSettingsSavedComposer(room.RoomId));
            room.SendPacket(new RoomInfoUpdatedComposer(room.RoomId));

            if (kickAll)
            {
                foreach (var roomUser in room.GetRoomUserManager().GetUserList().ToList())
                {
                    if (roomUser == null || roomUser.IsBot)
                    {
                        continue;
                    }

                    if (roomUser.GetClient() == null || roomUser.GetClient().GetHabbo() == null)
                    {
                        continue;
                    }

                    if (roomUser.GetClient().GetHabbo().Rank >= session.GetHabbo().Rank || roomUser.GetClient().GetHabbo().Id == session.GetHabbo().Id)
                    {
                        continue;
                    }

                    room.GetRoomUserManager().RemoveUserFromRoom(roomUser.GetClient(), true);
                }
            }
        }
    }
}
