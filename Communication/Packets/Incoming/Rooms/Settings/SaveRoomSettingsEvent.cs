﻿namespace Plus.Communication.Packets.Incoming.Rooms.Settings
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using Game.Navigator;
    using Game.Players;
    using Game.Rooms;
    using Outgoing.Navigator;
    using Outgoing.Rooms.Engine;
    using Outgoing.Rooms.Settings;

    internal class SaveRoomSettingsEvent : IPacketEvent
    {
        public void Parse(Player session, ClientPacket packet)
        {
            if (session == null || session.GetHabbo() == null)
            {
                return;
            }

            var roomId = packet.PopInt();

            if (!Program.GameContext.GetRoomManager().TryLoadRoom(roomId, out var room))
            {
                return;
            }

            var name = Program.GameContext.GetChatManager().GetFilter().CheckMessage(packet.PopString());
            var description = Program.GameContext.GetChatManager().GetFilter().CheckMessage(packet.PopString());
            var access = RoomAccessUtility.ToRoomAccess(packet.PopInt());
            var password = packet.PopString();
            var maxUsers = packet.PopInt();
            var categoryId = packet.PopInt();
            var tagCount = packet.PopInt();

            var tags = new List<string>();
            var formattedTags = new StringBuilder();

            for (var i = 0; i < tagCount; i++)
            {
                if (i > 0)
                {
                    formattedTags.Append(",");
                }

                var tag = packet.PopString().ToLower();

                tags.Add(tag);
                formattedTags.Append(tag);
            }

            var tradeSettings = packet.PopInt();//2 = All can trade, 1 = owner only, 0 = no trading.
            var allowPets = Convert.ToInt32(packet.PopBoolean() ? "1" : "0");
            var allowPetsEat = Convert.ToInt32(packet.PopBoolean() ? "1" : "0");
            var roomBlockingEnabled = Convert.ToInt32(packet.PopBoolean() ? "1" : "0");
            var hidewall = Convert.ToInt32(packet.PopBoolean() ? "1" : "0");
            var wallThickness = packet.PopInt();
            var floorThickness = packet.PopInt();
            var whoMute = packet.PopInt(); // mute
            var whoKick = packet.PopInt(); // kick
            var whoBan = packet.PopInt(); // ban

            var chatMode = packet.PopInt();
            var chatSize = packet.PopInt();
            var chatSpeed = packet.PopInt();
            var chatDistance = packet.PopInt();
            var extraFlood = packet.PopInt();

            if (chatMode < 0 || chatMode > 1)
            {
                chatMode = 0;
            }

            if (chatSize < 0 || chatSize > 2)
            {
                chatSize = 0;
            }

            if (chatSpeed < 0 || chatSpeed > 2)
            {
                chatSpeed = 0;
            }

            if (chatDistance < 0)
            {
                chatDistance = 1;
            }

            if (chatDistance > 99)
            {
                chatDistance = 100;
            }

            if (extraFlood < 0 || extraFlood > 2)
            {
                extraFlood = 0;
            }

            if (tradeSettings < 0 || tradeSettings > 2)
            {
                tradeSettings = 0;
            }

            if (whoMute < 0 || whoMute > 1)
            {
                whoMute = 0;
            }

            if (whoKick < 0 || whoKick > 1)
            {
                whoKick = 0;
            }

            if (whoBan < 0 || whoBan > 1)
            {
                whoBan = 0;
            }

            if (wallThickness < -2 || wallThickness > 1)
            {
                wallThickness = 0;
            }

            if (floorThickness < -2 || floorThickness > 1)
            {
                floorThickness = 0;
            }

            if (name.Length < 1)
            {
                return;
            }

            if (name.Length > 60)
            {
                name = name.Substring(0, 60);
            }

            if (access == RoomAccess.Password && password.Length == 0)
            {
                access = RoomAccess.Open;
            }

            if (maxUsers < 0)
            {
                maxUsers = 10;
            }

            if (maxUsers > 50)
            {
                maxUsers = 50;
            }

            if (!Program.GameContext.GetNavigator().TryGetSearchResultList(categoryId, out var searchResultList))
            {
                categoryId = 36;
            }

            if (searchResultList.CategoryType != NavigatorCategoryType.Category || searchResultList.RequiredRank > session.GetHabbo().Rank || session.GetHabbo().Id != room.OwnerId && session.GetHabbo().Rank >= searchResultList.RequiredRank)
            {
                categoryId = 36;
            }

            if (tagCount > 2)
            {
                return;
            }

            room.AllowPets = allowPets;
            room.AllowPetsEating = allowPetsEat;
            room.RoomBlockingEnabled = roomBlockingEnabled;
            room.Hidewall = hidewall;

            room.Name = name;
            room.Access = access;
            room.Description = description;
            room.Category = categoryId;
            room.Password = password;

            room.WhoCanBan = whoBan;
            room.WhoCanKick = whoKick;
            room.WhoCanMute = whoMute;

            room.ClearTags();
            room.AddTagRange(tags);
            room.UsersMax = maxUsers;

            room.WallThickness = wallThickness;
            room.FloorThickness = floorThickness;

            room.ChatMode = chatMode;
            room.ChatSize = chatSize;
            room.ChatSpeed = chatSpeed;
            room.ChatDistance = chatDistance;
            room.ExtraFlood = extraFlood;

            room.TradeSettings = tradeSettings;

            string accessStr;
            switch (access)
            {
                default:
                    accessStr = "open";
                    break;

                case RoomAccess.Password:
                    accessStr = "password";
                    break;

                case RoomAccess.Doorbell:
                    accessStr = "locked";
                    break;

                case RoomAccess.Invisible:
                    accessStr = "invisible";
                    break;
            }

            using (var dbClient = Program.DatabaseManager.GetQueryReactor())
            {
                dbClient.SetQuery("UPDATE `rooms` SET `caption` = @caption, `description` = @description, `password` = @password, `category` = @categoryId, `state` = @state, `tags` = @tags, `users_max` = @maxUsers, `allow_pets` = @allowPets, `allow_pets_eat` = @allowPetsEat, `room_blocking_disabled` = @roomBlockingDisabled, `allow_hidewall` = @allowHidewall, `floorthick` = @floorThick, `wallthick` = @wallThick, `mute_settings` = @muteSettings, `kick_settings` = @kickSettings, `ban_settings` = @banSettings, `chat_mode` = @chatMode, `chat_size` = @chatSize, `chat_speed` = @chatSpeed, `chat_extra_flood` = @extraFlood, `chat_hearing_distance` = @chatDistance, `trade_settings` = @tradeSettings WHERE `id` = @roomId LIMIT 1");
                dbClient.AddParameter("categoryId", categoryId);
                dbClient.AddParameter("maxUsers", maxUsers);
                dbClient.AddParameter("allowPets", allowPets.ToString());
                dbClient.AddParameter("allowPetsEat", allowPetsEat.ToString());
                dbClient.AddParameter("roomBlockingDisabled", roomBlockingEnabled.ToString());
                dbClient.AddParameter("allowHidewall", room.Hidewall.ToString());
                dbClient.AddParameter("floorThick", room.FloorThickness);
                dbClient.AddParameter("wallThick", room.WallThickness);
                dbClient.AddParameter("muteSettings", room.WhoCanMute.ToString());
                dbClient.AddParameter("kickSettings", room.WhoCanKick.ToString());
                dbClient.AddParameter("banSettings", room.WhoCanBan.ToString());
                dbClient.AddParameter("chatMode", room.ChatMode);
                dbClient.AddParameter("chatSize", room.ChatSize);
                dbClient.AddParameter("chatSpeed", room.ChatSpeed);
                dbClient.AddParameter("extraFlood", room.ExtraFlood);
                dbClient.AddParameter("chatDistance", room.ChatDistance);
                dbClient.AddParameter("tradeSettings", room.TradeSettings);
                dbClient.AddParameter("roomId", room.Id);
                dbClient.AddParameter("caption", room.Name);
                dbClient.AddParameter("description", room.Description);
                dbClient.AddParameter("password", room.Password);
                dbClient.AddParameter("state", accessStr);
                dbClient.AddParameter("tags", formattedTags.ToString());
                dbClient.RunQuery();
            }

            room.GetGameMap().GenerateMaps();

            if (session.GetHabbo().CurrentRoom == null)
            {
                session.SendPacket(new RoomSettingsSavedComposer(room.RoomId));
                session.SendPacket(new RoomInfoUpdatedComposer(room.RoomId));
                session.SendPacket(new RoomVisualizationSettingsComposer(room.WallThickness, room.FloorThickness, room.Hidewall.ToString() == "1"));
            }
            else
            {
                room.SendPacket(new RoomSettingsSavedComposer(room.RoomId));
                room.SendPacket(new RoomInfoUpdatedComposer(room.RoomId));
                room.SendPacket(new RoomVisualizationSettingsComposer(room.WallThickness, room.FloorThickness, room.Hidewall.ToString() == "1"));
            }
            
            Program.GameContext.GetAchievementManager().ProgressAchievement(session, "ACH_SelfModDoorModeSeen", 1);
            Program.GameContext.GetAchievementManager().ProgressAchievement(session, "ACH_SelfModWalkthroughSeen", 1);
            Program.GameContext.GetAchievementManager().ProgressAchievement(session, "ACH_SelfModChatScrollSpeedSeen", 1);
            Program.GameContext.GetAchievementManager().ProgressAchievement(session, "ACH_SelfModChatFloodFilterSeen", 1);
            Program.GameContext.GetAchievementManager().ProgressAchievement(session, "ACH_SelfModChatHearRangeSeen", 1);
        }
    }
}
