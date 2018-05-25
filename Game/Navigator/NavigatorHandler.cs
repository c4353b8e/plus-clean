namespace Plus.Game.Navigator
{
    using System.Collections.Generic;
    using System.Linq;
    using Communication.Packets.Outgoing;
    using Players;
    using Rooms;

    internal static class NavigatorHandler
    {
        public static void Search(ServerPacket Message, SearchResultList SearchResult, string SearchData, Player Session, int FetchLimit)
        {
            //Switching by categorys.
            switch (SearchResult.CategoryType)
            {
                default:
                    Message.WriteInteger(0);
                    break;

                case NavigatorCategoryType.Query:
                    {
                        #region Query
                        if (SearchData.ToLower().StartsWith("owner:"))
                        {
                            if (SearchData.Length > 0)
                            {
                                var UserId = 0;
                                var Results = new List<RoomData>();

                                using (var dbClient = Program.DatabaseManager.GetQueryReactor())
                                {
                                    if (SearchData.ToLower().StartsWith("owner:"))
                                    {
                                        dbClient.SetQuery("SELECT `id` FROM `users` WHERE `username` = @username LIMIT 1");
                                        dbClient.AddParameter("username", SearchData.Remove(0, 6));
                                        UserId = dbClient.GetInteger();

                                        dbClient.SetQuery("SELECT * FROM `rooms` WHERE `owner` = '" + UserId + "' and `state` != 'invisible' ORDER BY `users_now` DESC LIMIT 50");

                                        using (var reader = dbClient.ExecuteReader())
                                        {
                                            while (reader.Read())
                                            {
                                                RoomData Data = null;
                                                if (!RoomFactory.TryGetData(reader.GetInt32("id"), out Data))
                                                {
                                                    continue;
                                                }

                                                if (!Results.Contains(Data))
                                                {
                                                    Results.Add(Data);
                                                }
                                            }
                                        }
                                    }
                                }

                                Message.WriteInteger(Results.Count);
                                foreach (var Data in Results)
                                {
                                    RoomAppender.WriteRoom(Message, Data, Data.Promotion);
                                }
                            }
                        }
                        else if (SearchData.ToLower().StartsWith("tag:"))
                        {
                            SearchData = SearchData.Remove(0, 4);
                            ICollection<Room> TagMatches = Program.GameContext.GetRoomManager().SearchTaggedRooms(SearchData);

                            Message.WriteInteger(TagMatches.Count);
                            foreach (RoomData Data in TagMatches.ToList())
                            {
                                RoomAppender.WriteRoom(Message, Data, Data.Promotion);
                            }
                        }
                        else if (SearchData.ToLower().StartsWith("group:"))
                        {
                            SearchData = SearchData.Remove(0, 6);
                            ICollection<Room> GroupRooms = Program.GameContext.GetRoomManager().SearchGroupRooms(SearchData);

                            Message.WriteInteger(GroupRooms.Count);
                            foreach (RoomData Data in GroupRooms.ToList())
                            {
                                RoomAppender.WriteRoom(Message, Data, Data.Promotion);
                            }
                        }
                        else
                        {
                            if (SearchData.Length > 0)
                            {
                                var Results = new List<RoomData>();
                                using (var dbClient = Program.DatabaseManager.GetQueryReactor())
                                {
                                    dbClient.SetQuery("SELECT `id`,`caption`,`description`,`roomtype`,`owner`,`state`,`category`,`users_now`,`users_max`,`model_name`,`score`,`allow_pets`,`allow_pets_eat`,`room_blocking_disabled`,`allow_hidewall`,`password`,`wallpaper`,`floor`,`landscape`,`floorthick`,`wallthick`,`mute_settings`,`kick_settings`,`ban_settings`,`chat_mode`,`chat_speed`,`chat_size`,`trade_settings`,`group_id`,`tags`,`push_enabled`,`pull_enabled`,`enables_enabled`,`respect_notifications_enabled`,`pet_morphs_allowed`,`spush_enabled`,`spull_enabled` FROM rooms WHERE `caption` LIKE @query ORDER BY `users_now` DESC LIMIT 50");
                                    dbClient.AddParameter("query", "%" + SearchData + "%");
                                    using (var reader = dbClient.ExecuteReader())
                                    {
                                        while (reader.Read())
                                        {
                                            if (reader.GetString("state") == "invisible")
                                            {
                                                continue;
                                            }

                                            if (RoomFactory.TryGetData(reader.GetInt32("id"), out var Data) && !Results.Contains(Data))
                                            {
                                                Results.Add(Data);
                                            }
                                        }
                                    }
                                }

                                Message.WriteInteger(Results.Count);
                                foreach (var Data in Results)
                                {
                                    RoomAppender.WriteRoom(Message, Data, Data.Promotion);
                                }
                            }
                        }
                        #endregion

                        break;
                    }

                case NavigatorCategoryType.Featured:
                    #region Featured
                    var Rooms = new List<RoomData>();
                    var Featured = Program.GameContext.GetNavigator().GetFeaturedRooms();
                    foreach (var FeaturedItem in Featured.ToList())
                    {
                        if (FeaturedItem == null)
                        {
                            continue;
                        }

                        if (!RoomFactory.TryGetData(FeaturedItem.RoomId, out var Data))
                        {
                            continue;
                        }

                        if (!Rooms.Contains(Data))
                        {
                            Rooms.Add(Data);
                        }
                    }

                    Message.WriteInteger(Rooms.Count);
                    foreach (var Data in Rooms.ToList())
                    {
                        RoomAppender.WriteRoom(Message, Data, Data.Promotion);
                    }
                    #endregion
                    break;

                case NavigatorCategoryType.Popular:
                    {
                        var PopularRooms = Program.GameContext.GetRoomManager().GetPopularRooms(-1, FetchLimit);

                        Message.WriteInteger(PopularRooms.Count);
                        foreach (RoomData Data in PopularRooms.ToList())
                        {
                            RoomAppender.WriteRoom(Message, Data, Data.Promotion);
                        }
                        break;
                    }

                case NavigatorCategoryType.Recommended:
                    {
                        var RecommendedRooms = Program.GameContext.GetRoomManager().GetRecommendedRooms(FetchLimit);

                        Message.WriteInteger(RecommendedRooms.Count);
                        foreach (RoomData Data in RecommendedRooms.ToList())
                        {
                            RoomAppender.WriteRoom(Message, Data, Data.Promotion);
                        }
                        break;
                    }

                case NavigatorCategoryType.Category:
                    {
                        var GetRoomsByCategory = Program.GameContext.GetRoomManager().GetRoomsByCategory(SearchResult.Id, FetchLimit);

                        Message.WriteInteger(GetRoomsByCategory.Count);
                        foreach (RoomData Data in GetRoomsByCategory.ToList())
                        {
                            RoomAppender.WriteRoom(Message, Data, Data.Promotion);
                        }
                        break;
                    }

                case NavigatorCategoryType.MyRooms:

                    ICollection<RoomData> rooms = RoomFactory.GetRoomsDataByOwnerSortByName(Session.GetHabbo().Id).OrderByDescending(x => x.UsersNow).ToList();

                    Message.WriteInteger(rooms.Count);
                    foreach (var Data in rooms.ToList())
                    {
                        RoomAppender.WriteRoom(Message, Data, Data.Promotion);
                    }
                    break;

                case NavigatorCategoryType.MyFavourites:
                    var Favourites = new List<RoomData>();
                    foreach (int Id in Session.GetHabbo().FavoriteRooms.ToArray())
                    {
                        RoomData Room = null;
                        if (!RoomFactory.TryGetData(Id, out Room))
                        {
                            continue;
                        }

                        if (!Favourites.Contains(Room))
                        {
                            Favourites.Add(Room);
                        }
                    }

                    Favourites = Favourites.Take(FetchLimit).ToList();

                    Message.WriteInteger(Favourites.Count);
                    foreach (var Data in Favourites.ToList())
                    {
                        RoomAppender.WriteRoom(Message, Data, Data.Promotion);
                    }
                    break;

                case NavigatorCategoryType.MyGroups:
                    var MyGroups = new List<RoomData>();

                    foreach (var Group in Program.GameContext.GetGroupManager().GetGroupsForUser(Session.GetHabbo().Id).ToList())
                    {
                        if (Group == null)
                        {
                            continue;
                        }

                        RoomData Data = null;
                        if (!RoomFactory.TryGetData(Group.RoomId, out Data))
                        {
                            continue;
                        }

                        if (!MyGroups.Contains(Data))
                        {
                            MyGroups.Add(Data);
                        }
                    }

                    MyGroups = MyGroups.Take(FetchLimit).ToList();

                    Message.WriteInteger(MyGroups.Count);
                    foreach (var Data in MyGroups.ToList())
                    {
                        RoomAppender.WriteRoom(Message, Data, Data.Promotion);
                    }
                    break;

                case NavigatorCategoryType.MyFriendsRooms:
                    var MyFriendsRooms = new List<RoomData>();
                    foreach (var buddy in Session.GetHabbo().GetMessenger().GetFriends().Where(p => p.InRoom))
                    {
                        if (buddy == null || !buddy.InRoom || buddy.UserId == Session.GetHabbo().Id)
                        {
                            continue;
                        }

                        if (!MyFriendsRooms.Contains(buddy.CurrentRoom))
                        {
                            MyFriendsRooms.Add(buddy.CurrentRoom);
                        }
                    }

                    Message.WriteInteger(MyFriendsRooms.Count);
                    foreach (var Data in MyFriendsRooms.ToList())
                    {
                        RoomAppender.WriteRoom(Message, Data, Data.Promotion);
                    }
                    break;

                case NavigatorCategoryType.MyRights:
                    var MyRights = new List<RoomData>();

                    using (var dbClient = Program.DatabaseManager.GetQueryReactor())
                    {
                        dbClient.SetQuery("SELECT `room_id` FROM `room_rights` WHERE `user_id` = @UserId LIMIT @FetchLimit");
                        dbClient.AddParameter("UserId", Session.GetHabbo().Id);
                        dbClient.AddParameter("FetchLimit", FetchLimit);

                        using (var reader = dbClient.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                RoomData Data = null;
                                if (!RoomFactory.TryGetData(reader.GetInt32("room_id"), out Data))
                                {
                                    continue;
                                }

                                if (!MyRights.Contains(Data))
                                {
                                    MyRights.Add(Data);
                                }
                            }
                        }
                    }

                    Message.WriteInteger(MyRights.Count);
                    foreach (var Data in MyRights)
                    {
                        RoomAppender.WriteRoom(Message, Data, Data.Promotion);
                    }
                    break;

                case NavigatorCategoryType.TopPromotions:
                    {
                        var GetPopularPromotions = Program.GameContext.GetRoomManager().GetOnGoingRoomPromotions(16, FetchLimit);

                        Message.WriteInteger(GetPopularPromotions.Count);
                        foreach (RoomData Data in GetPopularPromotions.ToList())
                        {
                            RoomAppender.WriteRoom(Message, Data, Data.Promotion);
                        }
                        break;
                    }

                case NavigatorCategoryType.PromotionCategory:
                    {
                        var GetPromotedRooms = Program.GameContext.GetRoomManager().GetPromotedRooms(SearchResult.Id, FetchLimit);

                        Message.WriteInteger(GetPromotedRooms.Count);
                        foreach (RoomData Data in GetPromotedRooms.ToList())
                        {
                            RoomAppender.WriteRoom(Message, Data, Data.Promotion);
                        }
                        break;
                    }
            }
        }
    }
}