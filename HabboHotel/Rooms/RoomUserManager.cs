namespace Plus.HabboHotel.Rooms
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Linq;
    using AI;
    using Communication.Packets.Outgoing.Handshake;
    using Communication.Packets.Outgoing.Rooms.Avatar;
    using Communication.Packets.Outgoing.Rooms.Engine;
    using Communication.Packets.Outgoing.Rooms.Permissions;
    using Communication.Packets.Outgoing.Rooms.Session;
    using Core.Logging;
    using GameClients;
    using Games.Teams;
    using Items;
    using PathFinding;
    using Trading;
    using Utilities;

    public class RoomUserManager
    {
        private static readonly ILogger Logger = new Logger<RoomUserManager>();

        private Room _room;
        private ConcurrentDictionary<int, RoomUser> _users;
        private ConcurrentDictionary<int, RoomUser> _bots;
        private ConcurrentDictionary<int, RoomUser> _pets;

        private int primaryPrivateUserID;
        private int secondaryPrivateUserID;

        public int userCount;


        public RoomUserManager(Room room)
        {
            _room = room;
            _users = new ConcurrentDictionary<int, RoomUser>();
            _pets = new ConcurrentDictionary<int, RoomUser>();
            _bots = new ConcurrentDictionary<int, RoomUser>();

            primaryPrivateUserID = 0;
            secondaryPrivateUserID = 0;

            PetCount = 0;
            userCount = 0;
        }

        public RoomUser DeployBot(RoomBot bot, Pet pet)
        {
            var user = new RoomUser(0, _room.RoomId, primaryPrivateUserID++, _room);
            bot.VirtualId = primaryPrivateUserID;

            var PersonalID = secondaryPrivateUserID++;
            user.InternalRoomID = PersonalID;
            _users.TryAdd(PersonalID, user);

            var model = _room.GetGameMap().Model;

            if (bot.X > 0 && bot.Y > 0 && bot.X < model.MapSizeX && bot.Y < model.MapSizeY)
            {
                user.SetPos(bot.X, bot.Y, bot.Z);
                user.SetRot(bot.Rot, false);
            }
            else
            {
                bot.X = model.DoorX;
                bot.Y = model.DoorY;

                user.SetPos(model.DoorX, model.DoorY, model.DoorZ);
                user.SetRot(model.DoorOrientation, false);
            }

            user.BotData = bot;
            user.BotAI = bot.GenerateBotAI(user.VirtualId);

            if (user.IsPet)
            {
                user.BotAI.Init(bot.BotId, user.VirtualId, _room.RoomId, user, _room);
                user.PetData = pet;
                user.PetData.VirtualId = user.VirtualId;
            }
            else
            {
                user.BotAI.Init(bot.BotId, user.VirtualId, _room.RoomId, user, _room);
            }

            user.UpdateNeeded = true;

            _room.SendPacket(new UsersComposer(user));

            if (user.IsPet)
            {
                if (_pets.ContainsKey(user.PetData.PetId))
                {
                    _pets[user.PetData.PetId] = user;
                }
                else
                {
                    _pets.TryAdd(user.PetData.PetId, user);
                }

                PetCount++;
            }
            else if (user.IsBot)
            {
                if (_bots.ContainsKey(user.BotData.BotId))
                {
                    _bots[user.BotData.BotId] = user;
                }
                else
                {
                    _bots.TryAdd(user.BotData.Id, user);
                }

                _room.SendPacket(new DanceComposer(user, user.BotData.DanceId));
            }
            return user;
        }

        public void RemoveBot(int virtualId, bool kicked)
        {
            var user = GetRoomUserByVirtualId(virtualId);
            if (user == null || !user.IsBot)
            {
                return;
            }

            if (user.IsPet)
            {

                _pets.TryRemove(user.PetData.PetId, out var pet);
                PetCount--;
            }
            else
            {
                _bots.TryRemove(user.BotData.Id, out var bot);
            }

            user.BotAI.OnSelfLeaveRoom(kicked);

            _room.SendPacket(new UserRemoveComposer(user.VirtualId));

            _users?.TryRemove(user.InternalRoomID, out var toRemove);

            onRemove(user);
        }

        public RoomUser GetUserForSquare(int x, int y)
        {
            return _room.GetGameMap().GetRoomUsers(new Point(x, y)).FirstOrDefault();
        }

        public bool AddAvatarToRoom(GameClient session)
        {
            if (_room == null)
            {
                return false;
            }

            if (session == null)
            {
                return false;
            }

            if (session.GetHabbo().CurrentRoom == null)
            {
                return false;
            }

            var user = new RoomUser(session.GetHabbo().Id, _room.RoomId, primaryPrivateUserID++, _room);

            if (user == null || user.GetClient() == null)
            {
                return false;
            }

            user.UserId = session.GetHabbo().Id;

            session.GetHabbo().TentId = 0;

            var PersonalID = secondaryPrivateUserID++;
            user.InternalRoomID = PersonalID;


            session.GetHabbo().CurrentRoomId = _room.RoomId;
            if (!_users.TryAdd(PersonalID, user))
            {
                return false;
            }

            var model = _room.GetGameMap().Model;
            if (model == null)
            {
                return false;
            }

            if (!_room.PetMorphsAllowed && session.GetHabbo().PetId != 0)
            {
                session.GetHabbo().PetId = 0;
            }

            if (!session.GetHabbo().IsTeleporting && !session.GetHabbo().IsHopping)
            {
                if (!model.DoorIsValid())
                {
                    var Square = _room.GetGameMap().GetRandomWalkableSquare();
                    model.DoorX = Square.X;
                    model.DoorY = Square.Y;
                    model.DoorZ = _room.GetGameMap().GetHeightForSquareFromData(Square);
                }

                user.SetPos(model.DoorX, model.DoorY, model.DoorZ);
                user.SetRot(model.DoorOrientation, false);
            }
            else if (!user.IsBot && (user.GetClient().GetHabbo().IsTeleporting || user.GetClient().GetHabbo().IsHopping))
            {
                Item item = null;
                if (session.GetHabbo().IsTeleporting)
                {
                    item = _room.GetRoomItemHandler().GetItem(session.GetHabbo().TeleporterId);
                }
                else if (session.GetHabbo().IsHopping)
                {
                    item = _room.GetRoomItemHandler().GetItem(session.GetHabbo().HopperId);
                }

                if (item != null)
                {
                    if (session.GetHabbo().IsTeleporting)
                    {
                        item.ExtraData = "2";
                        item.UpdateState(false, true);
                        user.SetPos(item.GetX, item.GetY, item.GetZ);
                        user.SetRot(item.Rotation, false);
                        item.InteractingUser2 = session.GetHabbo().Id;
                        item.ExtraData = "0";
                        item.UpdateState(false, true);
                    }
                    else if (session.GetHabbo().IsHopping)
                    {
                        item.ExtraData = "1";
                        item.UpdateState(false, true);
                        user.SetPos(item.GetX, item.GetY, item.GetZ);
                        user.SetRot(item.Rotation, false);
                        user.AllowOverride = false;
                        item.InteractingUser2 = session.GetHabbo().Id;
                        item.ExtraData = "2";
                        item.UpdateState(false, true);
                    }
                }
                else
                {
                    user.SetPos(model.DoorX, model.DoorY, model.DoorZ - 1);
                    user.SetRot(model.DoorOrientation, false);
                }
            }

            _room.SendPacket(new UsersComposer(user));
            
            if (_room.CheckRights(session, true))
            {
                user.SetStatus("flatctrl", "useradmin");
                session.SendPacket(new YouAreOwnerComposer());
                session.SendPacket(new YouAreControllerComposer(4));
            }
            else if (_room.CheckRights(session, false) && _room.Group == null)
            {
                user.SetStatus("flatctrl", "1");
                session.SendPacket(new YouAreControllerComposer(1));
            }
            else if (_room.Group != null && _room.CheckRights(session, false, true))
            {
                user.SetStatus("flatctrl", "3");
                session.SendPacket(new YouAreControllerComposer(3));
            }
            else
            {
                session.SendPacket(new YouAreNotControllerComposer());
            }

            user.UpdateNeeded = true;

            if (session.GetHabbo().GetPermissions().HasRight("mod_tool") && !session.GetHabbo().DisableForcedEffects)
            {
                session.GetHabbo().Effects().ApplyEffect(102);
            }

            foreach (var bot in _bots.Values.ToList())
            {
                if (bot == null || bot.BotAI == null)
                {
                    continue;
                }

                bot.BotAI.OnUserEnterRoom(user);
            }
            return true;
        }

        public void RemoveUserFromRoom(GameClient session, bool nofityUser, bool notifyKick = false)
        {
            try
            {
                if (_room == null)
                {
                    return;
                }

                if (session == null || session.GetHabbo() == null)
                {
                    return;
                }

                if (notifyKick)
                {
                    session.SendPacket(new GenericErrorComposer(4008));
                }

                if (nofityUser)
                {
                    session.SendPacket(new CloseConnectionComposer());
                }

                if (session.GetHabbo().TentId > 0)
                {
                    session.GetHabbo().TentId = 0;
                }

                var User = GetRoomUserByHabbo(session.GetHabbo().Id);
                if (User != null)
                {
                    if (User.RidingHorse)
                    {
                        User.RidingHorse = false;
                        var UserRiding = GetRoomUserByVirtualId(User.HorseID);
                        if (UserRiding != null)
                        {
                            UserRiding.RidingHorse = false;
                            UserRiding.HorseID = 0;
                        }
                    }

                    if (User.Team != Team.None)
                    {
                        var Team = _room.GetTeamManagerForFreeze();
                        if (Team != null)
                        {
                            Team.OnUserLeave(User);

                            User.Team = Games.Teams.Team.None;

                            if (User.GetClient().GetHabbo().Effects().CurrentEffect != 0)
                            {
                                User.GetClient().GetHabbo().Effects().ApplyEffect(0);
                            }
                        }
                    }


                    RemoveRoomUser(User);

                    if (User.CurrentItemEffect != ItemEffectType.None)
                    {
                        if (session.GetHabbo().Effects() != null)
                        {
                            session.GetHabbo().Effects().CurrentEffect = -1;
                        }
                    }

                    if (User.IsTrading)
                    {
                        Trade Trade = null;
                        if (_room.GetTrading().TryGetTrade(User.TradeId, out Trade))
                        {
                            Trade.EndTrade(User.TradeId);
                        }
                    }

                    //Session.GetHabbo().CurrentRoomId = 0;

                    if (session.GetHabbo().GetMessenger() != null)
                    {
                        session.GetHabbo().GetMessenger().OnStatusChanged(true);
                    }

                    using (var dbClient = Program.DatabaseManager.GetQueryReactor())
                    {
                        dbClient.RunQuery("UPDATE user_roomvisits SET exit_timestamp = '" + UnixTimestamp.GetNow() + "' WHERE room_id = '" + _room.RoomId + "' AND user_id = '" + session.GetHabbo().Id + "' ORDER BY exit_timestamp DESC LIMIT 1");
                        dbClient.RunQuery("UPDATE `rooms` SET `users_now` = '" + _room.UsersNow + "' WHERE `id` = '" + _room.RoomId + "' LIMIT 1");
                    }

                    if (User != null)
                    {
                        User.Dispose();
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }
        }

        private void onRemove(RoomUser user)
        {
            try
            {

                var session = user.GetClient();
                if (session == null)
                {
                    return;
                }

                var Bots = new List<RoomUser>();

                try
                {
                    foreach (var roomUser in GetUserList().ToList())
                    {
                        if (roomUser == null)
                        {
                            continue;
                        }

                        if (roomUser.IsBot && !roomUser.IsPet)
                        {
                            if (!Bots.Contains(roomUser))
                            {
                                Bots.Add(roomUser);
                            }
                        }
                    }
                }
                catch { }

                var PetsToRemove = new List<RoomUser>();
                foreach (var Bot in Bots.ToList())
                {
                    if (Bot == null || Bot.BotAI == null)
                    {
                        continue;
                    }

                    Bot.BotAI.OnUserLeaveRoom(session);

                    if (Bot.IsPet && Bot.PetData.OwnerId == user.UserId && !_room.CheckRights(session, true))
                    {
                        if (!PetsToRemove.Contains(Bot))
                        {
                            PetsToRemove.Add(Bot);
                        }
                    }
                }

                foreach (var toRemove in PetsToRemove.ToList())
                {
                    if (toRemove == null)
                    {
                        continue;
                    }

                    if (user.GetClient() == null || user.GetClient().GetHabbo() == null || user.GetClient().GetHabbo().GetInventoryComponent() == null)
                    {
                        continue;
                    }

                    user.GetClient().GetHabbo().GetInventoryComponent().TryAddPet(toRemove.PetData);
                    RemoveBot(toRemove.VirtualId, false);
                }

                _room.GetGameMap().RemoveUserFromMap(user, new Point(user.X, user.Y));
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }
        }

        private void RemoveRoomUser(RoomUser user)
        {
            if (user.SetStep)
            {
                _room.GetGameMap().GameMap[user.SetX, user.SetY] = user.SqState;
            }
            else
            {
                _room.GetGameMap().GameMap[user.X, user.Y] = user.SqState;
            }

            _room.GetGameMap().RemoveUserFromMap(user, new Point(user.X, user.Y));
            _room.SendPacket(new UserRemoveComposer(user.VirtualId));

            RoomUser toRemove = null;
            if (_users.TryRemove(user.InternalRoomID, out toRemove))
            {
                //uhmm, could put the below stuff in but idk.
            }

            user.InternalRoomID = -1;
            onRemove(user);
        }

        public bool TryGetPet(int PetId, out RoomUser Pet)
        {
            return _pets.TryGetValue(PetId, out Pet);
        }

        public bool TryGetBot(int BotId, out RoomUser Bot)
        {
            return _bots.TryGetValue(BotId, out Bot);
        }

        public RoomUser GetBotByName(string Name)
        {
            var FoundBot = _bots.Count(x => x.Value.BotData != null && x.Value.BotData.Name.ToLower() == Name.ToLower()) > 0;
            if (FoundBot)
            {
                var Id = _bots.FirstOrDefault(x => x.Value.BotData != null && x.Value.BotData.Name.ToLower() == Name.ToLower()).Value.BotData.Id;

                return _bots[Id];
            }

            return null;
        }

        public void UpdateUserCount(int count)
        {
            userCount = count;

            using (var dbClient = Program.DatabaseManager.GetQueryReactor())
            {
                dbClient.RunQuery("UPDATE `rooms` SET `users_now` = '" + count + "' WHERE `id` = '" + _room.RoomId + "' LIMIT 1");
            }
        }

        public RoomUser GetRoomUserByVirtualId(int VirtualId)
        {
            RoomUser User = null;
            if (!_users.TryGetValue(VirtualId, out User))
            {
                return null;
            }

            return User;
        }

        public RoomUser GetRoomUserByHabbo(int Id)
        {
            var User = GetUserList().Where(x => x != null && x.GetClient() != null && x.GetClient().GetHabbo() != null && x.GetClient().GetHabbo().Id == Id).FirstOrDefault();

            if (User != null)
            {
                return User;
            }

            return null;
        }

        public List<RoomUser> GetRoomUsers()
        {
            var List = new List<RoomUser>();

            List = GetUserList().Where(x => !x.IsBot).ToList();

            return List;
        }

        public List<RoomUser> GetRoomUserByRank(int minRank)
        {
            var returnList = new List<RoomUser>();
            foreach (var user in GetUserList().ToList())
            {
                if (user == null)
                {
                    continue;
                }

                if (!user.IsBot && user.GetClient() != null && user.GetClient().GetHabbo() != null && user.GetClient().GetHabbo().Rank >= minRank)
                {
                    returnList.Add(user);
                }
            }

            return returnList;
        }

        public RoomUser GetRoomUserByHabbo(string pName)
        {
            var User = GetUserList().FirstOrDefault(x => x != null && x.GetClient() != null && x.GetClient().GetHabbo() != null && x.GetClient().GetHabbo().Username.Equals(pName, StringComparison.OrdinalIgnoreCase));
            if (User != null)
            {
                return User;
            }

            return null;
        }

        public void UpdatePets()
        {
            using (var dbClient = Program.DatabaseManager.GetQueryReactor())
            {
                foreach (var Pet in GetPets().ToList())
                {
                    if (Pet == null)
                    {
                        continue;
                    }

                    if (Pet.DBState == PetDatabaseUpdateState.NeedsInsert)
                    {
                        dbClient.SetQuery("INSERT INTO `bots` (`id`,`user_id`,`room_id`,`name`,`x`,`y`,`z`) VALUES ('" + Pet.PetId + "','" + Pet.OwnerId + "','" + Pet.RoomId + "',@name,'0','0','0')");
                        dbClient.AddParameter("name", Pet.Name);
                        dbClient.RunQuery();

                        dbClient.SetQuery("INSERT INTO `bots_petdata` (`type`,`race`,`color`,`experience`,`energy`,`createstamp`,`nutrition`,`respect`) VALUES ('" + Pet.Type + "',@race,@color,'0','100','" + Pet.CreationStamp + "','0','0')");
                        dbClient.AddParameter(Pet.PetId + "race", Pet.Race);
                        dbClient.AddParameter(Pet.PetId + "color", Pet.Color);
                        dbClient.RunQuery();
                    }
                    else if (Pet.DBState == PetDatabaseUpdateState.NeedsUpdate)
                    {
                        //Surely this can be *99 better? // TODO
                        var User = GetRoomUserByVirtualId(Pet.VirtualId);

                        dbClient.RunQuery("UPDATE `bots` SET room_id = " + Pet.RoomId + ", x = " + (User != null ? User.X : 0) + ", Y = " + (User != null ? User.Y : 0) + ", Z = " + (User != null ? User.Z : 0) + " WHERE `id` = '" + Pet.PetId + "' LIMIT 1");
                        dbClient.RunQuery("UPDATE `bots_petdata` SET `experience` = '" + Pet.experience + "', `energy` = '" + Pet.Energy + "', `nutrition` = '" + Pet.Nutrition + "', `respect` = '" + Pet.Respect + "' WHERE `id` = '" + Pet.PetId + "' LIMIT 1");
                    }

                    Pet.DBState = PetDatabaseUpdateState.Updated;
                }
            }
        }

        private void UpdateBots()
        {
            using (var dbClient = Program.DatabaseManager.GetQueryReactor())
            {
                foreach (var User in GetRoomUsers().ToList())
                {
                    if (User == null || !User.IsBot)
                    {
                        continue;
                    }

                    if (User.IsBot)
                    {
                        dbClient.SetQuery("UPDATE bots SET x=@x, y=@y, z=@z, name=@name, look=@look, rotation=@rotation WHERE id=@id LIMIT 1;");
                        dbClient.AddParameter("name", User.BotData.Name);
                        dbClient.AddParameter("look", User.BotData.Look);
                        dbClient.AddParameter("rotation", User.BotData.Rot);
                        dbClient.AddParameter("x", User.X);
                        dbClient.AddParameter("y", User.Y);
                        dbClient.AddParameter("z", User.Z);
                        dbClient.AddParameter("id", User.BotData.BotId);
                        dbClient.RunQuery();
                    }
                }
            }
        }


        public List<Pet> GetPets()
        {
            var Pets = new List<Pet>();
            foreach (var User in _pets.Values.ToList())
            {
                if (User == null || !User.IsPet)
                {
                    continue;
                }

                Pets.Add(User.PetData);
            }

            return Pets;
        }

        public void SerializeStatusUpdates()
        {
            var Users = new List<RoomUser>();
            var RoomUsers = GetUserList();

            if (RoomUsers == null)
            {
                return;
            }

            foreach (var User in RoomUsers.ToList())
            {
                if (User == null || !User.UpdateNeeded || Users.Contains(User))
                {
                    continue;
                }

                User.UpdateNeeded = false;
                Users.Add(User);
            }

            if (Users.Count > 0)
            {
                _room.SendPacket(new UserUpdateComposer(Users));
            }
        }

        public void UpdateUserStatusses()
        {
            foreach (var user in GetUserList().ToList())
            {
                if (user == null)
                {
                    continue;
                }

                UpdateUserStatus(user, false);
            }
        }

        private bool IsValid(RoomUser user)
        {
            if (user == null)
            {
                return false;
            }

            if (user.IsBot)
            {
                return true;
            }

            if (user.GetClient() == null)
            {
                return false;
            }

            return user.GetClient().GetHabbo() != null && user.GetClient().GetHabbo().CurrentRoomId == _room.RoomId;
        }

        public void OnCycle()
        {
            var userCounter = 0;

            try
            {

                var ToRemove = new List<RoomUser>();

                foreach (var User in GetUserList().ToList())
                {
                    if (User == null)
                    {
                        continue;
                    }

                    if (!IsValid(User))
                    {
                        if (User.GetClient() != null)
                        {
                            RemoveUserFromRoom(User.GetClient(), false);
                        }
                        else
                        {
                            RemoveRoomUser(User);
                        }
                    }

                    if (User.NeedsAutokick && !ToRemove.Contains(User))
                    {
                        ToRemove.Add(User);
                        continue;
                    }

                    var updated = false;
                    User.IdleTime++;
                    User.HandleSpamTicks();

                    if (!User.IsBot && !User.IsAsleep && User.IdleTime >= 600)
                    {
                        User.IsAsleep = true;
                        _room.SendPacket(new SleepComposer(User, true));
                    }

                    if (User.CarryItemId > 0)
                    {
                        User.CarryTimer--;
                        if (User.CarryTimer <= 0)
                        {
                            User.CarryItem(0);
                        }
                    }

                    if (_room.GotFreeze())
                    {
                        _room.GetFreeze().CycleUser(User);
                    }

                    var InvalidStep = false;

                    if (User.isRolling)
                    {
                        if (User.rollerDelay <= 0)
                        {
                            UpdateUserStatus(User, false);
                            User.isRolling = false;
                        }
                        else
                        {
                            User.rollerDelay--;
                        }
                    }

                    if (User.SetStep)
                    {
                        if (_room.GetGameMap().IsValidStep2(User, new Vector2D(User.X, User.Y), new Vector2D(User.SetX, User.SetY), User.GoalX == User.SetX && User.GoalY == User.SetY, User.AllowOverride))
                        {
                            if (!User.RidingHorse)
                            {
                                _room.GetGameMap().UpdateUserMovement(new Point(User.Coordinate.X, User.Coordinate.Y), new Point(User.SetX, User.SetY), User);
                            }

                            var items = _room.GetGameMap().GetCoordinatedItems(new Point(User.X, User.Y));
                            foreach (var Item in items.ToList())
                            {
                                Item.UserWalksOffFurni(User);
                            }

                            if (!User.RidingHorse)
                            {
                                User.X = User.SetX;
                                User.Y = User.SetY;
                                User.Z = User.SetZ;
                            }
                            
                            if (!User.IsBot && User.RidingHorse)
                            {
                                var Horse = GetRoomUserByVirtualId(User.HorseID);
                                if (Horse != null)
                                {
                                    Horse.X = User.SetX;
                                    Horse.Y = User.SetY;
                                }
                            }

                            if (User.X == _room.GetGameMap().Model.DoorX && User.Y == _room.GetGameMap().Model.DoorY && !ToRemove.Contains(User) && !User.IsBot)
                            {
                                ToRemove.Add(User);
                                continue;
                            }

                            var Items = _room.GetGameMap().GetCoordinatedItems(new Point(User.X, User.Y));
                            foreach (var Item in Items.ToList())
                            {
                                Item.UserWalksOnFurni(User);
                            }

                            UpdateUserStatus(User, true);
                        }
                        else
                        {
                            InvalidStep = true;
                        }

                        User.SetStep = false;
                    }

                    if (User.PathRecalcNeeded)
                    {
                        User.Path = PathFinder.FindPath(User, _room.GetGameMap().DiagonalEnabled, _room.GetGameMap(), new Vector2D(User.X, User.Y), new Vector2D(User.GoalX, User.GoalY));

                        if (User.Path.Count > 1)
                        {
                            User.PathStep = 1;
                            User.IsWalking = true;
                        }

                        User.PathRecalcNeeded = false;
                    }

                    if (User.IsWalking && !User.Freezed)
                    {
                        if (InvalidStep || User.PathStep >= User.Path.Count || User.GoalX == User.X && User.GoalY == User.Y) //No path found, or reached goal (:
                        {
                            User.IsWalking = false;
                            User.RemoveStatus("mv");

                            if (User.Statusses.ContainsKey("sign"))
                            {
                                User.RemoveStatus("sign");
                            }

                            if (User.IsBot && User.BotData.TargetUser > 0)
                            {
                                if (User.CarryItemId > 0)
                                {
                                    var Target = _room.GetRoomUserManager().GetRoomUserByHabbo(User.BotData.TargetUser);

                                    if (Target != null && Gamemap.TilesTouching(User.X, User.Y, Target.X, Target.Y))
                                    {
                                        User.SetRot(Rotation.Calculate(User.X, User.Y, Target.X, Target.Y), false);
                                        Target.SetRot(Rotation.Calculate(Target.X, Target.Y, User.X, User.Y), false);
                                        Target.CarryItem(User.CarryItemId);
                                    }
                                }

                                User.CarryItem(0);
                                User.BotData.TargetUser = 0;
                            }

                            if (User.RidingHorse && User.IsPet == false && !User.IsBot)
                            {
                                var mascotaVinculada = GetRoomUserByVirtualId(User.HorseID);
                                if (mascotaVinculada != null)
                                {
                                    mascotaVinculada.IsWalking = false;
                                    mascotaVinculada.RemoveStatus("mv");
                                    mascotaVinculada.UpdateNeeded = true;
                                }
                            }
                        }
                        else
                        {
                            var NextStep = User.Path[User.Path.Count - User.PathStep - 1];
                            User.PathStep++;

                            if (!_room.GetGameMap().IsValidStep(new Vector2D(User.X, User.Y), new Vector2D(NextStep.X, NextStep.Y), User.GoalX == User.SetX && User.GoalY == User.SetY, User.AllowOverride))
                            {
                                User.Path = PathFinder.FindPath(User, _room.GetGameMap().DiagonalEnabled, _room.GetGameMap(), new Vector2D(User.X, User.Y), new Vector2D(User.GoalX, User.GoalY));

                                if (User.Path.Count > 1)
                                {
                                    User.PathStep = 1;
                                    User.IsWalking = true;
                                    User.PathRecalcNeeded = false;
                                }

                                User.PathRecalcNeeded = false;
                                NextStep = User.Path[User.Path.Count - User.PathStep - 1];
                            }

                            if (User.FastWalking && User.PathStep < User.Path.Count)
                            {
                                var s2 = User.Path.Count - User.PathStep - 1;
                                NextStep = User.Path[s2];
                                User.PathStep++;
                            }

                            if (User.SuperFastWalking && User.PathStep < User.Path.Count)
                            {
                                var s2 = User.Path.Count - User.PathStep - 1;
                                NextStep = User.Path[s2];
                                User.PathStep++;
                                User.PathStep++;
                            }

                            var nextX = NextStep.X;
                            var nextY = NextStep.Y;
                            User.RemoveStatus("mv");

                            if (_room.GetGameMap().IsValidStep2(User, new Vector2D(User.X, User.Y), new Vector2D(nextX, nextY), User.GoalX == nextX && User.GoalY == nextY, User.AllowOverride))
                            {
                                var nextZ = _room.GetGameMap().SqAbsoluteHeight(nextX, nextY);

                                if (!User.IsBot)
                                {
                                    if (User.isSitting)
                                    {
                                        User.isSitting = false;
                                    }
                                    else if (User.isLying)
                                    {
                                        User.isLying = false;
                                    }
                                    if (User.isSitting || User.isLying)
                                    {
                                        User.Z += 0.35;
                                        User.UpdateNeeded = true;
                                    }
                                    User.Statusses.Remove("lay");
                                    User.Statusses.Remove("sit");
                                }

                                if (!User.IsBot && !User.IsPet && User.GetClient() != null)
                                {
                                    if (User.GetClient().GetHabbo().IsTeleporting)
                                    {
                                        User.GetClient().GetHabbo().IsTeleporting = false;
                                        User.GetClient().GetHabbo().TeleporterId = 0;
                                    }
                                    else if (User.GetClient().GetHabbo().IsHopping)
                                    {
                                        User.GetClient().GetHabbo().IsHopping = false;
                                        User.GetClient().GetHabbo().HopperId = 0;
                                    }
                                }

                                if (!User.IsBot && User.RidingHorse && User.IsPet == false)
                                {
                                    var Horse = GetRoomUserByVirtualId(User.HorseID);
                                    if (Horse != null)
                                    {
                                        Horse.SetStatus("mv", nextX + "," + nextY + "," + TextHandling.GetString(nextZ));
                                    }

                                    User.SetStatus("mv", +nextX + "," + nextY + "," + TextHandling.GetString(nextZ + 1));

                                    User.UpdateNeeded = true;
                                    Horse.UpdateNeeded = true;
                                }
                                else
                                {
                                    User.SetStatus("mv", nextX + "," + nextY + "," + TextHandling.GetString(nextZ));
                                }


                                var newRot = Rotation.Calculate(User.X, User.Y, nextX, nextY, User.moonwalkEnabled);

                                User.RotBody = newRot;
                                User.RotHead = newRot;

                                User.SetStep = true;
                                User.SetX = nextX;
                                User.SetY = nextY;
                                User.SetZ = nextZ;
                                UpdateUserEffect(User, User.SetX, User.SetY);

                                updated = true;

                                if (User.RidingHorse && User.IsPet == false && !User.IsBot)
                                {
                                    var Horse = GetRoomUserByVirtualId(User.HorseID);
                                    if (Horse != null)
                                    {
                                        Horse.RotBody = newRot;
                                        Horse.RotHead = newRot;

                                        Horse.SetStep = true;
                                        Horse.SetX = nextX;
                                        Horse.SetY = nextY;
                                        Horse.SetZ = nextZ;
                                    }
                                }

                                _room.GetGameMap().GameMap[User.X, User.Y] = User.SqState; // REstore the old one
                                User.SqState = _room.GetGameMap().GameMap[User.SetX, User.SetY]; //Backup the new one

                                if (_room.RoomBlockingEnabled == 0)
                                {
                                    var Users = _room.GetRoomUserManager().GetUserForSquare(nextX, nextY);
                                    if (Users != null)
                                    {
                                        _room.GetGameMap().GameMap[nextX, nextY] = 0;
                                    }
                                }
                                else
                                {
                                    _room.GetGameMap().GameMap[nextX, nextY] = 1;
                                }
                            }
                        }
                        if (!User.RidingHorse)
                        {
                            User.UpdateNeeded = true;
                        }
                    }
                    else
                    {
                        if (User.Statusses.ContainsKey("mv"))
                        {
                            User.RemoveStatus("mv");
                            User.UpdateNeeded = true;

                            if (User.RidingHorse)
                            {
                                var Horse = GetRoomUserByVirtualId(User.HorseID);
                                if (Horse != null)
                                {
                                    Horse.RemoveStatus("mv");
                                    Horse.UpdateNeeded = true;
                                }
                            }
                        }
                    }

                    if (User.RidingHorse)
                    {
                        User.ApplyEffect(77);
                    }

                    if (User.IsBot && User.BotAI != null)
                    {
                        User.BotAI.OnTimerTick();
                    }
                    else
                    {
                        userCounter++;
                    }

                    if (!updated)
                    {
                        UpdateUserEffect(User, User.X, User.Y);
                    }
                }

                foreach (var toRemove in ToRemove.ToList())
                {
                    var client = Program.GameContext.GetClientManager().GetClientByUserId(toRemove.HabboId);
                    if (client != null)
                    {
                        RemoveUserFromRoom(client, true);
                    }
                    else
                    {
                        RemoveRoomUser(toRemove);
                    }
                }

                if (userCount != userCounter)
                {
                    UpdateUserCount(userCounter);
                }
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }
        }

        public void UpdateUserStatus(RoomUser User, bool cyclegameitems)
        {
            if (User == null)
            {
                return;
            }

            try
            {
                var isBot = User.IsBot;
                if (isBot)
                {
                    cyclegameitems = false;
                }

                if (UnixTimestamp.GetNow() > UnixTimestamp.GetNow() + User.SignTime)
                {
                    if (User.Statusses.ContainsKey("sign"))
                    {
                        User.Statusses.Remove("sign");
                        User.UpdateNeeded = true;
                    }
                }

                if (User.Statusses.ContainsKey("lay") && !User.isLying || User.Statusses.ContainsKey("sit") && !User.isSitting)
                {
                    if (User.Statusses.ContainsKey("lay"))
                    {
                        User.Statusses.Remove("lay");
                    }

                    if (User.Statusses.ContainsKey("sit"))
                    {
                        User.Statusses.Remove("sit");
                    }

                    User.UpdateNeeded = true;
                }
                else if (User.isLying || User.isSitting)
                {
                    return;
                }

                double newZ;
                var ItemsOnSquare = _room.GetGameMap().GetAllRoomItemForSquare(User.X, User.Y);
                if (ItemsOnSquare != null || ItemsOnSquare.Count != 0)
                {
                    if (User.RidingHorse && User.IsPet == false)
                    {
                        newZ = _room.GetGameMap().SqAbsoluteHeight(User.X, User.Y, ItemsOnSquare.ToList()) + 1;
                    }
                    else
                    {
                        newZ = _room.GetGameMap().SqAbsoluteHeight(User.X, User.Y, ItemsOnSquare.ToList());
                    }
                }
                else
                {
                    newZ = 1;
                }

                if (newZ != User.Z)
                {
                    User.Z = newZ;
                    User.UpdateNeeded = true;
                }

                var Model = _room.GetGameMap().Model;
                if (Model.SqState[User.X, User.Y] == SquareState.Seat)
                {
                    if (!User.Statusses.ContainsKey("sit"))
                    {
                        User.Statusses.Add("sit", "1.0");
                    }

                    User.Z = Model.SqFloorHeight[User.X, User.Y];
                    User.RotHead = Model.SqSeatRot[User.X, User.Y];
                    User.RotBody = Model.SqSeatRot[User.X, User.Y];

                    User.UpdateNeeded = true;
                }


                if (ItemsOnSquare.Count == 0)
                {
                    User.LastItem = null;
                }


                foreach (var Item in ItemsOnSquare.ToList())
                {
                    if (Item == null)
                    {
                        continue;
                    }

                    if (Item.GetBaseItem().IsSeat)
                    {
                        if (!User.Statusses.ContainsKey("sit"))
                        {
                            if (!User.Statusses.ContainsKey("sit"))
                            {
                                User.Statusses.Add("sit", TextHandling.GetString(Item.GetBaseItem().Height));
                            }
                        }

                        User.Z = Item.GetZ;
                        User.RotHead = Item.Rotation;
                        User.RotBody = Item.Rotation;
                        User.UpdateNeeded = true;
                    }

                    switch (Item.GetBaseItem().InteractionType)
                    {
                        #region Beds & Tents
                        case InteractionType.BED:
                        case InteractionType.TENT_SMALL:
                            {
                                if (!User.Statusses.ContainsKey("lay"))
                                {
                                    User.Statusses.Add("lay", TextHandling.GetString(Item.GetBaseItem().Height) + " null");
                                }

                                User.Z = Item.GetZ;
                                User.RotHead = Item.Rotation;
                                User.RotBody = Item.Rotation;

                                User.UpdateNeeded = true;
                                break;
                            }
                        #endregion

                        #region Banzai Gates
                        case InteractionType.banzaigategreen:
                        case InteractionType.banzaigateblue:
                        case InteractionType.banzaigatered:
                        case InteractionType.banzaigateyellow:
                            {
                                if (cyclegameitems)
                                {
                                    var effectID = Convert.ToInt32(Item.team + 32);
                                    var t = User.GetClient().GetHabbo().CurrentRoom.GetTeamManagerForBanzai();

                                    if (User.Team == Team.None)
                                    {
                                        if (t.CanEnterOnTeam(Item.team))
                                        {
                                            if (User.Team != Team.None)
                                            {
                                                t.OnUserLeave(User);
                                            }

                                            User.Team = Item.team;

                                            t.AddUser(User);

                                            if (User.GetClient().GetHabbo().Effects().CurrentEffect != effectID)
                                            {
                                                User.GetClient().GetHabbo().Effects().ApplyEffect(effectID);
                                            }
                                        }
                                    }
                                    else if (User.Team != Team.None && User.Team != Item.team)
                                    {
                                        t.OnUserLeave(User);
                                        User.Team = Team.None;
                                        User.GetClient().GetHabbo().Effects().ApplyEffect(0);
                                    }
                                    else
                                    {
                                        //usersOnTeam--;
                                        t.OnUserLeave(User);
                                        if (User.GetClient().GetHabbo().Effects().CurrentEffect == effectID)
                                        {
                                            User.GetClient().GetHabbo().Effects().ApplyEffect(0);
                                        }

                                        User.Team = Team.None;
                                    }
                                    //Item.ExtraData = usersOnTeam.ToString();
                                    //Item.UpdateState(false, true);                                
                                }
                                break;
                            }
                        #endregion

                        #region Freeze Gates
                        case InteractionType.FREEZE_YELLOW_GATE:
                        case InteractionType.FREEZE_RED_GATE:
                        case InteractionType.FREEZE_GREEN_GATE:
                        case InteractionType.FREEZE_BLUE_GATE:
                            {
                                if (cyclegameitems)
                                {
                                    var effectID = Convert.ToInt32(Item.team + 39);
                                    var t = User.GetClient().GetHabbo().CurrentRoom.GetTeamManagerForFreeze();

                                    if (User.Team == Team.None)
                                    {
                                        if (t.CanEnterOnTeam(Item.team))
                                        {
                                            if (User.Team != Team.None)
                                            {
                                                t.OnUserLeave(User);
                                            }

                                            User.Team = Item.team;
                                            t.AddUser(User);

                                            if (User.GetClient().GetHabbo().Effects().CurrentEffect != effectID)
                                            {
                                                User.GetClient().GetHabbo().Effects().ApplyEffect(effectID);
                                            }
                                        }
                                    }
                                    else if (User.Team != Team.None && User.Team != Item.team)
                                    {
                                        t.OnUserLeave(User);
                                        User.Team = Team.None;
                                        User.GetClient().GetHabbo().Effects().ApplyEffect(0);
                                    }
                                    else
                                    {
                                        //usersOnTeam--;
                                        t.OnUserLeave(User);
                                        if (User.GetClient().GetHabbo().Effects().CurrentEffect == effectID)
                                        {
                                            User.GetClient().GetHabbo().Effects().ApplyEffect(0);
                                        }

                                        User.Team = Team.None;
                                    }
                                    //Item.ExtraData = usersOnTeam.ToString();
                                    //Item.UpdateState(false, true);                                
                                }
                                break;
                            }
                        #endregion

                        #region Banzai Teles
                        case InteractionType.banzaitele:
                            {
                                if (User.Statusses.ContainsKey("mv"))
                                {
                                    _room.GetGameItemHandler().OnTeleportRoomUserEnter(User, Item);
                                }

                                break;
                            }
                        #endregion

                        #region Football Gate

                        #endregion

                        #region Effects
                        case InteractionType.EFFECT:
                            {
                                if (User == null)
                                {
                                    return;
                                }

                                if (!User.IsBot)
                                {
                                    if (Item?.GetBaseItem() == null || User.GetClient() == null || User.GetClient().GetHabbo() == null || User.GetClient().GetHabbo().Effects() == null)
                                    {
                                        return;
                                    }

                                    if (Item.GetBaseItem().EffectId == 0 && User.GetClient().GetHabbo().Effects().CurrentEffect == 0)
                                    {
                                        return;
                                    }

                                    User.GetClient().GetHabbo().Effects().ApplyEffect(Item.GetBaseItem().EffectId);
                                    Item.ExtraData = "1";
                                    Item.UpdateState(false, true);
                                    Item.RequestUpdate(2, true);
                                }
                                break;
                            }
                        #endregion

                        #region Arrows
                        case InteractionType.ARROW:
                            {
                                if (User.GoalX == Item.GetX && User.GoalY == Item.GetY)
                                {
                                    if (User == null || User.GetClient() == null || User.GetClient().GetHabbo() == null)
                                    {
                                        continue;
                                    }

                                    Room Room;

                                    if (!Program.GameContext.GetRoomManager().TryGetRoom(User.GetClient().GetHabbo().CurrentRoomId, out Room))
                                    {
                                        return;
                                    }

                                    if (!ItemTeleporterFinder.IsTeleLinked(Item.Id, Room))
                                    {
                                        User.UnlockWalking();
                                    }
                                    else
                                    {
                                        var LinkedTele = ItemTeleporterFinder.GetLinkedTele(Item.Id);
                                        var TeleRoomId = ItemTeleporterFinder.GetTeleRoomId(LinkedTele, Room);

                                        if (TeleRoomId == Room.RoomId)
                                        {
                                            var TargetItem = Room.GetRoomItemHandler().GetItem(LinkedTele);
                                            if (TargetItem == null)
                                            {
                                                if (User.GetClient() != null)
                                                {
                                                    User.GetClient().SendWhisper("Hey, that arrow is poorly!");
                                                }

                                                return;
                                            }

                                            Room.GetGameMap().TeleportToItem(User, TargetItem);
                                        }
                                        else if (TeleRoomId != Room.RoomId)
                                        {
                                            if (User != null && !User.IsBot && User.GetClient() != null && User.GetClient().GetHabbo() != null)
                                            {
                                                User.GetClient().GetHabbo().IsTeleporting = true;
                                                User.GetClient().GetHabbo().TeleportingRoomID = TeleRoomId;
                                                User.GetClient().GetHabbo().TeleporterId = LinkedTele;

                                                User.GetClient().GetHabbo().PrepareRoom(TeleRoomId, "");
                                            }
                                        }
                                        else if (_room.GetRoomItemHandler().GetItem(LinkedTele) != null)
                                        {
                                            User.SetPos(Item.GetX, Item.GetY, Item.GetZ);
                                            User.SetRot(Item.Rotation, false);
                                        }
                                        else
                                        {
                                            User.UnlockWalking();
                                        }
                                    }
                                }
                                break;
                            }
                            #endregion

                    }
                }

                if (User.isSitting && User.TeleportEnabled)
                {
                    User.Z -= 0.35;
                    User.UpdateNeeded = true;
                }

                if (cyclegameitems)
                {
                    if (_room.GotSoccer())
                    {
                        _room.GetSoccer().OnUserWalk(User);
                    }

                    if (_room.GotBanzai())
                    {
                        _room.GetBanzai().OnUserWalk(User);
                    }

                    if (_room.GotFreeze())
                    {
                        _room.GetFreeze().OnUserWalk(User);
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }
        }

        private void UpdateUserEffect(RoomUser User, int x, int y)
        {
            if (User == null || User.IsBot || User.GetClient() == null || User.GetClient().GetHabbo() == null)
            {
                return;
            }

            try
            {
                var NewCurrentUserItemEffect = _room.GetGameMap().EffectMap[x, y];
                if (NewCurrentUserItemEffect > 0)
                {
                    if (User.GetClient().GetHabbo().Effects().CurrentEffect == 0)
                    {
                        User.CurrentItemEffect = ItemEffectType.None;
                    }

                    var Type = ByteToItemEffectEnum.Parse(NewCurrentUserItemEffect);
                    if (Type != User.CurrentItemEffect)
                    {
                        switch (Type)
                        {
                            case ItemEffectType.Iceskates:
                                {
                                    User.GetClient().GetHabbo().Effects().ApplyEffect(User.GetClient().GetHabbo().Gender == "M" ? 38 : 39);
                                    User.CurrentItemEffect = ItemEffectType.Iceskates;
                                    break;
                                }

                            case ItemEffectType.Normalskates:
                                {
                                    User.GetClient().GetHabbo().Effects().ApplyEffect(User.GetClient().GetHabbo().Gender == "M" ? 55 : 56);
                                    User.CurrentItemEffect = Type;
                                    break;
                                }
                            case ItemEffectType.Swim:
                                {
                                    User.GetClient().GetHabbo().Effects().ApplyEffect(29);
                                    User.CurrentItemEffect = Type;
                                    break;
                                }
                            case ItemEffectType.SwimLow:
                                {
                                    User.GetClient().GetHabbo().Effects().ApplyEffect(30);
                                    User.CurrentItemEffect = Type;
                                    break;
                                }
                            case ItemEffectType.SwimHalloween:
                                {
                                    User.GetClient().GetHabbo().Effects().ApplyEffect(37);
                                    User.CurrentItemEffect = Type;
                                    break;
                                }

                            case ItemEffectType.None:
                                {
                                    User.GetClient().GetHabbo().Effects().ApplyEffect(-1);
                                    User.CurrentItemEffect = Type;
                                    break;
                                }
                        }
                    }
                }
                else if (User.CurrentItemEffect != ItemEffectType.None && NewCurrentUserItemEffect == 0)
                {
                    User.GetClient().GetHabbo().Effects().ApplyEffect(-1);
                    User.CurrentItemEffect = ItemEffectType.None;
                }
            }
            catch
            {
            }
        }

        public int PetCount { get; private set; }

        public ICollection<RoomUser> GetUserList()
        {
            return _users.Values;
        }

        public void Dispose()
        {
            UpdatePets();
            UpdateBots();

            _room.UsersNow = 0;
            using (var dbClient = Program.DatabaseManager.GetQueryReactor())
            {
                dbClient.RunQuery("UPDATE `rooms` SET `users_now` = '0' WHERE `id` = '" + _room.Id + "' LIMIT 1");
            }

            _users.Clear();
            _pets.Clear();
            _bots.Clear();

            userCount = 0;
            PetCount = 0;

            _users = null;
            _pets = null;
            _bots = null;
            _room = null;
        }
    }
}