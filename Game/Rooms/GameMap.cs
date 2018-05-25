namespace Plus.Game.Rooms
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Linq;
    using Core.Logging;
    using Games.Teams;
    using Items;
    using PathFinding;
    using Utilities;

    public class Gamemap
    {
        private static readonly ILogger Logger = new Logger<Gamemap>();

        private Room _room;

        public bool DiagonalEnabled { get; set; }
        private double[,] _itemHeightMap;
        private ConcurrentDictionary<Point, List<int>> _coordinatedItems;
        private ConcurrentDictionary<Point, List<RoomUser>> _userMap;

        public Gamemap(Room room, RoomModel model)
        {
            _room = room;
            StaticModel = model;
            DiagonalEnabled = true;

            Model = new DynamicRoomModel(StaticModel);
            _coordinatedItems = new ConcurrentDictionary<Point, List<int>>();
            _itemHeightMap = new double[Model.MapSizeX, Model.MapSizeY];
            _userMap = new ConcurrentDictionary<Point, List<RoomUser>>();
        }

        public void AddUserToMap(RoomUser user, Point coord)
        {
            if (_userMap.ContainsKey(coord))
            {
                _userMap[coord].Add(user);
            }
            else
            {
                var users = new List<RoomUser>
                {
                    user
                };
                _userMap.TryAdd(coord, users);
            }
        }

        public void TeleportToItem(RoomUser user, Item item)
        {
            if (item == null || user == null)
            {
                return;
            }

            GameMap[user.X, user.Y] = user.SqState;
            UpdateUserMovement(new Point(user.Coordinate.X, user.Coordinate.Y), new Point(item.Coordinate.X, item.Coordinate.Y), user);
            user.X = item.GetX;
            user.Y = item.GetY;
            user.Z = item.GetZ;

            user.SqState = GameMap[item.GetX, item.GetY];
            GameMap[user.X, user.Y] = 1;
            user.RotBody = item.Rotation;
            user.RotHead = item.Rotation;

            user.GoalX = user.X;
            user.GoalY = user.Y;
            user.SetStep = false;
            user.IsWalking = false;
            user.UpdateNeeded = true;
        }

        public void UpdateUserMovement(Point oldCoord, Point newCoord, RoomUser user)
        {
            RemoveUserFromMap(user, oldCoord);
            AddUserToMap(user, newCoord);
        }

        public void RemoveUserFromMap(RoomUser user, Point coord)
        {
            if (_userMap.ContainsKey(coord))
            {
                _userMap[coord].RemoveAll(x => x != null && x.VirtualId == user.VirtualId);
            }
        }

        public bool MapGotUser(Point coord)
        {
            return GetRoomUsers(coord).Count > 0;
        }

        public List<RoomUser> GetRoomUsers(Point coord)
        {
            if (_userMap.ContainsKey(coord))
            {
                return _userMap[coord];
            }

            return new List<RoomUser>();
        }

        public Point GetRandomWalkableSquare()
        {
            var walkableSquares = new List<Point>();
            for (var y = 0; y < GameMap.GetUpperBound(1); y++)
            {
                for (var x = 0; x < GameMap.GetUpperBound(0); x++)
                {
                    if (StaticModel.DoorX != x && StaticModel.DoorY != y && GameMap[x, y] == 1)
                    {
                        walkableSquares.Add(new Point(x, y));
                    }
                }
            }

            var random = RandomNumber.GenerateNewRandom(0, walkableSquares.Count);
            var i = 0;

            foreach (var coord in walkableSquares.ToList())
            {
                if (i == random)
                {
                    return coord;
                }

                i++;
            }

            return new Point(0, 0);
        }
        
        public void AddToMap(Item item)
        {
            AddItemToMap(item);
        }

        private void SetDefaultValue(int x, int y)
        {
            GameMap[x, y] = 0;
            EffectMap[x, y] = 0;
            _itemHeightMap[x, y] = 0.0;

            if (x == Model.DoorX && y == Model.DoorY)
            {
                GameMap[x, y] = 3;
            }
            else if (Model.SqState[x, y] == SquareState.Open)
            {
                GameMap[x, y] = 1;
            }
            else if (Model.SqState[x, y] == SquareState.Seat)
            {
                GameMap[x, y] = 2;
            }
        }

        public void UpdateMapForItem(Item item)
        {
            RemoveFromMap(item);
            AddToMap(item);
        }

        public void GenerateMaps(bool checkLines = true)
        {
            var MaxX = 0;
            var MaxY = 0;
            _coordinatedItems = new ConcurrentDictionary<Point, List<int>>();

            if (checkLines)
            {
                var items = _room.GetRoomItemHandler().GetFloor.ToArray();
                foreach (var item in items.ToList())
                {
                    if (item == null)
                    {
                        continue;
                    }

                    if (item.GetX > Model.MapSizeX && item.GetX > MaxX)
                    {
                        MaxX = item.GetX;
                    }

                    if (item.GetY > Model.MapSizeY && item.GetY > MaxY)
                    {
                        MaxY = item.GetY;
                    }
                }

                Array.Clear(items, 0, items.Length);
                items = null;
            }


            if (MaxY > Model.MapSizeY - 1 || MaxX > Model.MapSizeX - 1)
            {
                if (MaxX < Model.MapSizeX)
                {
                    MaxX = Model.MapSizeX;
                }

                if (MaxY < Model.MapSizeY)
                {
                    MaxY = Model.MapSizeY;
                }

                Model.SetMapsize(MaxX + 7, MaxY + 7);
                GenerateMaps(false);
                return;
            }

            if (MaxX != StaticModel.MapSizeX || MaxY != StaticModel.MapSizeY)
            {
                EffectMap = new byte[Model.MapSizeX, Model.MapSizeY];
                GameMap = new byte[Model.MapSizeX, Model.MapSizeY];


                _itemHeightMap = new double[Model.MapSizeX, Model.MapSizeY];
                //if (modelRemap)
                //    Model.Generate(); //Clears model

                for (var line = 0; line < Model.MapSizeY; line++)
                {
                    for (var chr = 0; chr < Model.MapSizeX; chr++)
                    {
                        GameMap[chr, line] = 0;
                        EffectMap[chr, line] = 0;

                        if (chr == Model.DoorX && line == Model.DoorY)
                        {
                            GameMap[chr, line] = 3;
                        }
                        else if (Model.SqState[chr, line] == SquareState.Open)
                        {
                            GameMap[chr, line] = 1;
                        }
                        else if (Model.SqState[chr, line] == SquareState.Seat)
                        {
                            GameMap[chr, line] = 2;
                        }
                        else if (Model.SqState[chr, line] == SquareState.Pool)
                        {
                            EffectMap[chr, line] = 6;
                        }
                    }
                }
            }
            else
            {
                EffectMap = new byte[Model.MapSizeX, Model.MapSizeY];
                GameMap = new byte[Model.MapSizeX, Model.MapSizeY];


                _itemHeightMap = new double[Model.MapSizeX, Model.MapSizeY];

                for (var line = 0; line < Model.MapSizeY; line++)
                {
                    for (var chr = 0; chr < Model.MapSizeX; chr++)
                    {
                        GameMap[chr, line] = 0;
                        EffectMap[chr, line] = 0;

                        if (chr == Model.DoorX && line == Model.DoorY)
                        {
                            GameMap[chr, line] = 3;
                        }
                        else if (Model.SqState[chr, line] == SquareState.Open)
                        {
                            GameMap[chr, line] = 1;
                        }
                        else if (Model.SqState[chr, line] == SquareState.Seat)
                        {
                            GameMap[chr, line] = 2;
                        }
                        else if (Model.SqState[chr, line] == SquareState.Pool)
                        {
                            EffectMap[chr, line] = 6;
                        }
                    }
                }
            }

            var tmpItems = _room.GetRoomItemHandler().GetFloor.ToArray();
            foreach (var Item in tmpItems.ToList())
            {
                if (Item == null)
                {
                    continue;
                }

                if (!AddItemToMap(Item))
                {
                }
            }
            Array.Clear(tmpItems, 0, tmpItems.Length);
            tmpItems = null;

            if (_room.RoomBlockingEnabled == 0)
            {
                foreach (var user in _room.GetRoomUserManager().GetUserList().ToList())
                {
                    if (user == null)
                    {
                        continue;
                    }

                    user.SqState = GameMap[user.X, user.Y];
                    GameMap[user.X, user.Y] = 0;
                }
            }

            try
            {
                GameMap[Model.DoorX, Model.DoorY] = 3;
            }
            catch { }
        }

        private bool ConstructMapForItem(Item Item, Point Coord)
        {
            try
            {
                if (Coord.X > Model.MapSizeX - 1)
                {
                    Model.AddX();
                    GenerateMaps();
                    return false;
                }

                if (Coord.Y > Model.MapSizeY - 1)
                {
                    Model.AddY();
                    GenerateMaps();
                    return false;
                }

                if (Model.SqState[Coord.X, Coord.Y] == SquareState.Blocked)
                {
                    Model.OpenSquare(Coord.X, Coord.Y, Item.GetZ);
                }
                if (_itemHeightMap[Coord.X, Coord.Y] <= Item.TotalHeight)
                {
                    _itemHeightMap[Coord.X, Coord.Y] = Item.TotalHeight - Model.SqFloorHeight[Item.GetX, Item.GetY];
                    EffectMap[Coord.X, Coord.Y] = 0;


                    switch (Item.GetBaseItem().InteractionType)
                    {
                        case InteractionType.POOL:
                            EffectMap[Coord.X, Coord.Y] = 1;
                            break;
                        case InteractionType.NORMAL_SKATES:
                            EffectMap[Coord.X, Coord.Y] = 2;
                            break;
                        case InteractionType.ICE_SKATES:
                            EffectMap[Coord.X, Coord.Y] = 3;
                            break;
                        case InteractionType.lowpool:
                            EffectMap[Coord.X, Coord.Y] = 4;
                            break;
                        case InteractionType.haloweenpool:
                            EffectMap[Coord.X, Coord.Y] = 5;
                            break;
                    }


                    //SwimHalloween
                    if (Item.GetBaseItem().Walkable)    // If this item is walkable and on the floor, allow users to walk here.
                    {
                        if (GameMap[Coord.X, Coord.Y] != 3)
                        {
                            GameMap[Coord.X, Coord.Y] = 1;
                        }
                    }
                    else if (Item.GetZ <= Model.SqFloorHeight[Item.GetX, Item.GetY] + 0.1 && Item.GetBaseItem().InteractionType == InteractionType.GATE && Item.ExtraData == "1")// If this item is a gate, open, and on the floor, allow users to walk here.
                    {
                        if (GameMap[Coord.X, Coord.Y] != 3)
                        {
                            GameMap[Coord.X, Coord.Y] = 1;
                        }
                    }
                    else if (Item.GetBaseItem().IsSeat || Item.GetBaseItem().InteractionType == InteractionType.BED || Item.GetBaseItem().InteractionType == InteractionType.TENT_SMALL)
                    {
                        GameMap[Coord.X, Coord.Y] = 3;
                    }
                    else // Finally, if it's none of those, block the square.
                    {
                        if (GameMap[Coord.X, Coord.Y] != 3)
                        {
                            GameMap[Coord.X, Coord.Y] = 0;
                        }
                    }
                }

                // Set bad maps
                if (Item.GetBaseItem().InteractionType == InteractionType.BED || Item.GetBaseItem().InteractionType == InteractionType.TENT_SMALL)
                {
                    GameMap[Coord.X, Coord.Y] = 3;
                }
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }
            return true;
        }

        public void AddCoordinatedItem(Item item, Point coord)
        {
            var Items = new List<int>();

            if (!_coordinatedItems.TryGetValue(coord, out Items))
            {
                Items = new List<int>();

                if (!Items.Contains(item.Id))
                {
                    Items.Add(item.Id);
                }

                if (!_coordinatedItems.ContainsKey(coord))
                {
                    _coordinatedItems.TryAdd(coord, Items);
                }
            }
            else
            {
                if (!Items.Contains(item.Id))
                {
                    Items.Add(item.Id);
                    _coordinatedItems[coord] = Items;
                }
            }
        }

        public List<Item> GetCoordinatedItems(Point coord)
        {
            var point = new Point(coord.X, coord.Y);
            var Items = new List<Item>();

            if (_coordinatedItems.ContainsKey(point))
            {
                var Ids = _coordinatedItems[point];
                Items = GetItemsFromIds(Ids);
                return Items;
            }

            return new List<Item>();
        }

        public bool RemoveCoordinatedItem(Item item, Point coord)
        {
            var point = new Point(coord.X, coord.Y);
            if (_coordinatedItems != null && _coordinatedItems.ContainsKey(point))
            {
                _coordinatedItems[point].RemoveAll(x => x == item.Id);
                return true;
            }
            return false;
        }

        private void AddSpecialItems(Item item)
        {
            switch (item.GetBaseItem().InteractionType)
            {
                case InteractionType.FOOTBALL_GATE:
                    //IsTrans = true;
                    _room.GetSoccer().RegisterGate(item);


                    var splittedExtraData = item.ExtraData.Split(':');

                    if (string.IsNullOrEmpty(item.ExtraData) || splittedExtraData.Length <= 1)
                    {
                        item.Gender = "M";
                        switch (item.team)
                        {
                            case Team.Yellow:
                                item.Figure = "lg-275-93.hr-115-61.hd-207-14.ch-265-93.sh-305-62";
                                break;
                            case Team.Red:
                                item.Figure = "lg-275-96.hr-115-61.hd-180-3.ch-265-96.sh-305-62";
                                break;
                            case Team.Green:
                                item.Figure = "lg-275-102.hr-115-61.hd-180-3.ch-265-102.sh-305-62";
                                break;
                            case Team.Blue:
                                item.Figure = "lg-275-108.hr-115-61.hd-180-3.ch-265-108.sh-305-62";
                                break;
                        }
                    }
                    else
                    {
                        item.Gender = splittedExtraData[0];
                        item.Figure = splittedExtraData[1];
                    }
                    break;

                case InteractionType.banzaifloor:
                    {
                        _room.GetBanzai().AddTile(item, item.Id);
                        break;
                    }

                case InteractionType.banzaipyramid:
                    {
                        _room.GetGameItemHandler().AddPyramid(item, item.Id);
                        break;
                    }

                case InteractionType.banzaitele:
                    {
                        _room.GetGameItemHandler().AddTeleport(item, item.Id);
                        item.ExtraData = "";
                        break;
                    }
                case InteractionType.banzaipuck:
                    {
                        _room.GetBanzai().AddPuck(item);
                        break;
                    }

                case InteractionType.FOOTBALL:
                    {
                        _room.GetSoccer().AddBall(item);
                        break;
                    }
                case InteractionType.FREEZE_TILE_BLOCK:
                    {
                        _room.GetFreeze().AddFreezeBlock(item);
                        break;
                    }
                case InteractionType.FREEZE_TILE:
                    {
                        _room.GetFreeze().AddFreezeTile(item);
                        break;
                    }
                case InteractionType.freezeexit:
                    {
                        _room.GetFreeze().AddExitTile(item);
                        break;
                    }
            }
        }

        private void RemoveSpecialItem(Item item)
        {
            switch (item.GetBaseItem().InteractionType)
            {
                case InteractionType.FOOTBALL_GATE:
                    _room.GetSoccer().UnRegisterGate(item);
                    break;
                case InteractionType.banzaifloor:
                    _room.GetBanzai().RemoveTile(item.Id);
                    break;
                case InteractionType.banzaipuck:
                    _room.GetBanzai().RemovePuck(item.Id);
                    break;
                case InteractionType.banzaipyramid:
                    _room.GetGameItemHandler().RemovePyramid(item.Id);
                    break;
                case InteractionType.banzaitele:
                    _room.GetGameItemHandler().RemoveTeleport(item.Id);
                    break;
                case InteractionType.FOOTBALL:
                    _room.GetSoccer().RemoveBall(item.Id);
                    break;
                case InteractionType.FREEZE_TILE:
                    _room.GetFreeze().RemoveFreezeTile(item.Id);
                    break;
                case InteractionType.FREEZE_TILE_BLOCK:
                    _room.GetFreeze().RemoveFreezeBlock(item.Id);
                    break;
                case InteractionType.freezeexit:
                    _room.GetFreeze().RemoveExitTile(item.Id);
                    break;
            }
        }

        public bool RemoveFromMap(Item item, bool handleGameItem)
        {
            if (handleGameItem)
            {
                RemoveSpecialItem(item);
            }

            if (_room.GotSoccer())
            {
                _room.GetSoccer().OnGateRemove(item);
            }

            var isRemoved = false;
            foreach (var coord in item.GetCoords.ToList())
            {
                if (RemoveCoordinatedItem(item, coord))
                {
                    isRemoved = true;
                }
            }

            var items = new ConcurrentDictionary<Point, List<Item>>();
            foreach (var Tile in item.GetCoords.ToList())
            {
                var point = new Point(Tile.X, Tile.Y);
                if (_coordinatedItems.ContainsKey(point))
                {
                    var Ids = _coordinatedItems[point];
                    var __items = GetItemsFromIds(Ids);

                    if (!items.ContainsKey(Tile))
                    {
                        items.TryAdd(Tile, __items);
                    }
                }

                SetDefaultValue(Tile.X, Tile.Y);
            }

            foreach (var Coord in items.Keys.ToList())
            {
                if (!items.ContainsKey(Coord))
                {
                    continue;
                }

                var SubItems = items[Coord];
                foreach (var Item in SubItems.ToList())
                {
                    ConstructMapForItem(Item, Coord);
                }
            }


            items.Clear();
            items = null;


            return isRemoved;
        }

        public bool RemoveFromMap(Item item)
        {
            return RemoveFromMap(item, true);
        }

        public bool AddItemToMap(Item Item, bool handleGameItem, bool NewItem = true)
        {

            if (handleGameItem)
            {
                AddSpecialItems(Item);

                switch (Item.GetBaseItem().InteractionType)
                {
                    case InteractionType.FOOTBALL_GOAL_RED:
                    case InteractionType.footballcounterred:
                    case InteractionType.banzaiscorered:
                    case InteractionType.banzaigatered:
                    case InteractionType.freezeredcounter:
                    case InteractionType.FREEZE_RED_GATE:
                        {
                            if (!_room.GetRoomItemHandler().GetFloor.Contains(Item))
                            {
                                _room.GetGameManager().AddFurnitureToTeam(Item, Team.Red);
                            }

                            break;
                        }
                    case InteractionType.FOOTBALL_GOAL_GREEN:
                    case InteractionType.footballcountergreen:
                    case InteractionType.banzaiscoregreen:
                    case InteractionType.banzaigategreen:
                    case InteractionType.freezegreencounter:
                    case InteractionType.FREEZE_GREEN_GATE:
                        {
                            if (!_room.GetRoomItemHandler().GetFloor.Contains(Item))
                            {
                                _room.GetGameManager().AddFurnitureToTeam(Item, Team.Green);
                            }

                            break;
                        }
                    case InteractionType.FOOTBALL_GOAL_BLUE:
                    case InteractionType.footballcounterblue:
                    case InteractionType.banzaiscoreblue:
                    case InteractionType.banzaigateblue:
                    case InteractionType.freezebluecounter:
                    case InteractionType.FREEZE_BLUE_GATE:
                        {
                            if (!_room.GetRoomItemHandler().GetFloor.Contains(Item))
                            {
                                _room.GetGameManager().AddFurnitureToTeam(Item, Team.Blue);
                            }

                            break;
                        }
                    case InteractionType.FOOTBALL_GOAL_YELLOW:
                    case InteractionType.footballcounteryellow:
                    case InteractionType.banzaiscoreyellow:
                    case InteractionType.banzaigateyellow:
                    case InteractionType.freezeyellowcounter:
                    case InteractionType.FREEZE_YELLOW_GATE:
                        {
                            if (!_room.GetRoomItemHandler().GetFloor.Contains(Item))
                            {
                                _room.GetGameManager().AddFurnitureToTeam(Item, Team.Yellow);
                            }

                            break;
                        }
                    case InteractionType.freezeexit:
                        {
                            _room.GetFreeze().AddExitTile(Item);
                            break;
                        }
                    case InteractionType.ROLLER:
                        {
                            if (!_room.GetRoomItemHandler().GetRollers().Contains(Item))
                            {
                                _room.GetRoomItemHandler().TryAddRoller(Item.Id, Item);
                            }

                            break;
                        }
                }
            }

            if (Item.GetBaseItem().Type != 's')
            {
                return true;
            }

            foreach (var coord in Item.GetCoords.ToList())
            {
                AddCoordinatedItem(Item, new Point(coord.X, coord.Y));
            }

            if (Item.GetX > Model.MapSizeX - 1)
            {
                Model.AddX();
                GenerateMaps();
                return false;
            }

            if (Item.GetY > Model.MapSizeY - 1)
            {
                Model.AddY();
                GenerateMaps();
                return false;
            }

            var Return = true;

            foreach (var coord in Item.GetCoords)
            {
                if (!ConstructMapForItem(Item, coord))
                {
                    Return = false;
                }
                else
                {
                    Return = true;
                }
            }



            return Return;
        }


        public bool CanWalk(int X, int Y, bool Override)
        {

            if (Override)
            {
                return true;
            }

            if (_room.GetRoomUserManager().GetUserForSquare(X, Y) != null && _room.RoomBlockingEnabled == 0)
            {
                return false;
            }

            return true;
        }

        public bool AddItemToMap(Item Item, bool NewItem = true)
        {
            return AddItemToMap(Item, true, NewItem);
        }

        public bool ItemCanMove(Item Item, Point MoveTo)
        {
            var Points = GetAffectedTiles(Item.GetBaseItem().Length, Item.GetBaseItem().Width, MoveTo.X, MoveTo.Y, Item.Rotation).Values.ToList();

            if (Points == null || Points.Count == 0)
            {
                return true;
            }

            foreach (var Coord in Points)
            {

                if (Coord.X >= Model.MapSizeX || Coord.Y >= Model.MapSizeY)
                {
                    return false;
                }

                if (!SquareIsOpen(Coord.X, Coord.Y, false))
                {
                    return false;
                }
            }

            return true;
        }

        public byte GetFloorStatus(Point coord)
        {
            if (coord.X > GameMap.GetUpperBound(0) || coord.Y > GameMap.GetUpperBound(1))
            {
                return 1;
            }

            return GameMap[coord.X, coord.Y];
        }

        public double GetHeightForSquareFromData(Point coord)
        {
            if (coord.X > Model.SqFloorHeight.GetUpperBound(0) ||
                coord.Y > Model.SqFloorHeight.GetUpperBound(1))
            {
                return 1;
            }

            return Model.SqFloorHeight[coord.X, coord.Y];
        }

        public bool CanRollItemHere(int x, int y)
        {
            if (!ValidTile(x, y))
            {
                return false;
            }

            if (Model.SqState[x, y] == SquareState.Blocked)
            {
                return false;
            }

            return true;
        }

        public bool SquareIsOpen(int x, int y, bool pOverride)
        {
            if (Model.MapSizeX - 1 < x || Model.MapSizeY - 1 < y)
            {
                return false;
            }

            return CanWalk(GameMap[x, y], pOverride);
        }

        public bool GetHighestItemForSquare(Point Square, out Item Item)
        {
            var Items = GetAllRoomItemForSquare(Square.X, Square.Y);
            Item = null;
            double HighestZ = -1;

            if (Items != null && Items.Count() > 0)
            {
                foreach (var uItem in Items.ToList())
                {
                    if (uItem == null)
                    {
                        continue;
                    }

                    if (uItem.TotalHeight > HighestZ)
                    {
                        HighestZ = uItem.TotalHeight;
                        Item = uItem;
                    }
                }
            }
            else
            {
                return false;
            }

            return true;
        }

        public double GetHeightForSquare(Point coord)
        {
            if (GetHighestItemForSquare(coord, out var rItem))
            {
                if (rItem != null)
                {
                    return rItem.TotalHeight;
                }
            }

            return 0.0;
        }

        public Point GetChaseMovement(Item item)
        {
            var Distance = 99;
            var Coord = new Point(0, 0);
            var iX = item.GetX;
            var iY = item.GetY;
            var X = false;

            foreach (var User in _room.GetRoomUserManager().GetRoomUsers())
            {
                if (User.X == item.GetX || item.GetY == User.Y)
                {
                    if (User.X == item.GetX)
                    {
                        var Difference = Math.Abs(User.Y - item.GetY);
                        if (Difference < Distance)
                        {
                            Distance = Difference;
                            Coord = User.Coordinate;
                            X = false;
                        }
                    }
                    else if (User.Y == item.GetY)
                    {
                        var Difference = Math.Abs(User.X - item.GetX);
                        if (Difference < Distance)
                        {
                            Distance = Difference;
                            Coord = User.Coordinate;
                            X = true;
                        }
                    }
                }
            }

            if (Distance > 5)
            {
                return item.GetSides().OrderBy(x => Guid.NewGuid()).FirstOrDefault();
            }

            if (X && Distance < 99)
            {
                if (iX > Coord.X)
                {
                    iX--;
                    return new Point(iX, iY);
                }

                iX++;
                return new Point(iX, iY);
            }

            if (!X && Distance < 99)
            {
                if (iY > Coord.Y)
                {
                    iY--;
                    return new Point(iX, iY);
                }

                iY++;
                return new Point(iX, iY);
            }

            return item.Coordinate;
        }

        public bool IsValidStep2(RoomUser User, Vector2D From, Vector2D To, bool EndOfPath, bool Override)
        {
            if (User == null)
            {
                return false;
            }

            if (!ValidTile(To.X, To.Y))
            {
                return false;
            }

            if (Override)
            {
                return true;
            }
            
            var Items = _room.GetGameMap().GetAllRoomItemForSquare(To.X, To.Y);

            if (Items.Count > 0)
            {
                var HasGroupGate = Items.Count(x => x.GetBaseItem().InteractionType == InteractionType.GUILD_GATE) > 0;

                if (HasGroupGate)
                {
                    var I = Items.FirstOrDefault(x => x.GetBaseItem().InteractionType == InteractionType.GUILD_GATE);
                    if (I != null)
                    {
                        if (!Program.GameContext.GetGroupManager().TryGetGroup(I.GroupId, out var Group))
                        {
                            return false;
                        }

                        if (User.GetClient() == null || User.GetClient().GetHabbo() == null)
                        {
                            return false;
                        }

                        if (Group.IsMember(User.GetClient().GetHabbo().Id))
                        {
                            I.InteractingUser = User.GetClient().GetHabbo().Id;
                            I.ExtraData = "1";
                            I.UpdateState(false, true);

                            I.RequestUpdate(4, true);

                            return true;
                        }

                        if (User.Path.Count > 0)
                        {
                            User.Path.Clear();
                        }

                        User.PathRecalcNeeded = false;
                        return false;
                    }
                }
            }

            var Chair = false;
            double HighestZ = -1;
            foreach (var Item in Items.ToList())
            {
                if (Item == null)
                {
                    continue;
                }

                if (Item.GetZ < HighestZ)
                {
                    Chair = false;
                    continue;
                }

                HighestZ = Item.GetZ;
                if (Item.GetBaseItem().IsSeat)
                {
                    Chair = true;
                }
            }

            if (GameMap[To.X, To.Y] == 3 && !EndOfPath && !Chair || GameMap[To.X, To.Y] == 0 || GameMap[To.X, To.Y] == 2 && !EndOfPath)
            {
                if (User.Path.Count > 0)
                {
                    User.Path.Clear();
                }

                User.PathRecalcNeeded = true;
            }

            var HeightDiff = SqAbsoluteHeight(To.X, To.Y) - SqAbsoluteHeight(From.X, From.Y);
            if (HeightDiff > 1.5 && !User.RidingHorse)
            {
                return false;
            }

            //Check this last, because ya.
            var Userx = _room.GetRoomUserManager().GetUserForSquare(To.X, To.Y);
            if (Userx != null)
            {
                if (!Userx.IsWalking && EndOfPath)
                {
                    return false;
                }
            }
            return true;
        }
    
        public bool IsValidStep(Vector2D from, Vector2D to, bool endOfPath, bool overriding, bool roller = false)
        {
            if (!ValidTile(to.X, to.Y))
            {
                return false;
            }

            if (overriding)
            {
                return true;
            }

            /*
             * 0 = blocked
             * 1 = open
             * 2 = last step
             * 3 = door
             * */

            if (_room.RoomBlockingEnabled == 0 && SquareHasUsers(to.X, to.Y))
            {
                return false;
            }

            var items = _room.GetGameMap().GetAllRoomItemForSquare(to.X, to.Y);
            if (items.Count > 0)
            {
                var HasGroupGate = items.ToList().Count(x => x != null && x.GetBaseItem().InteractionType == InteractionType.GUILD_GATE) > 0;
                if (HasGroupGate)
                {
                    return true;
                }
            }

            if (GameMap[to.X, to.Y] == 3 && !endOfPath || GameMap[to.X, to.Y] == 0 || GameMap[to.X, to.Y] == 2 && !endOfPath)
            {
                return false;
            }

            if (!roller)
            {
                var HeightDiff = SqAbsoluteHeight(to.X, to.Y) - SqAbsoluteHeight(from.X, from.Y);
                if (HeightDiff > 1.5)
                {
                    return false;
                }
            }

            return true;
        }

        public static bool CanWalk(byte state, bool overriding)
        {
            if (!overriding)
            {
                if (state == 3)
                {
                    return true;
                }

                if (state == 1)
                {
                    return true;
                }

                return false;
            }
            return true;
        }

        public bool ItemCanBePlaced(int x, int y)
        {
            if (Model.MapSizeX - 1 < x || Model.MapSizeY - 1 < y ||
                x == Model.DoorX && y == Model.DoorY)
            {
                return false;
            }

            return GameMap[x, y] == 1;
        }

        public double SqAbsoluteHeight(int x, int y)
        {
            var Points = new Point(x, y);


            if (_coordinatedItems.TryGetValue(Points, out var Ids))
            {
                var Items = GetItemsFromIds(Ids);

                return SqAbsoluteHeight(x, y, Items);
            }

            return Model.SqFloorHeight[x, y];

            #region Old
            /*
            if (mCoordinatedItems.ContainsKey(Points))
            {
                List<Item> Items = new List<Item>();
                foreach (Item Item in mCoordinatedItems[Points].ToArray())
                {
                    if (!Items.Contains(Item))
                        Items.Add(Item);
                }
                return SqAbsoluteHeight(X, Y, Items);
            }*/
            #endregion
        }

        public double SqAbsoluteHeight(int X, int Y, List<Item> ItemsOnSquare)
        {
            try
            {
                var deduct = false;
                double HighestStack = 0;
                var deductable = 0.0;

                if (ItemsOnSquare != null && ItemsOnSquare.Count > 0)
                {
                    foreach (var Item in ItemsOnSquare.ToList())
                    {
                        if (Item == null)
                        {
                            continue;
                        }

                        if (Item.TotalHeight > HighestStack)
                        {
                            if (Item.GetBaseItem().IsSeat || Item.GetBaseItem().InteractionType == InteractionType.BED || Item.GetBaseItem().InteractionType == InteractionType.TENT_SMALL)
                            {
                                deduct = true;
                                deductable = Item.GetBaseItem().Height;
                            }
                            else
                            {
                                deduct = false;
                            }

                            HighestStack = Item.TotalHeight;
                        }
                    }
                }

                double floorHeight = Model.SqFloorHeight[X, Y];
                var stackHeight = HighestStack - Model.SqFloorHeight[X, Y];

                if (deduct)
                {
                    stackHeight -= deductable;
                }

                if (stackHeight < 0)
                {
                    stackHeight = 0;
                }

                return floorHeight + stackHeight;
            }
            catch (Exception e)
            {
                Logger.Error(e);
                return 0;
            }
        }

        public bool ValidTile(int X, int Y)
        {
            if (X < 0 || Y < 0 || X >= Model.MapSizeX || Y >= Model.MapSizeY)
            {
                return false;
            }

            return true;
        }

        public static Dictionary<int, ThreeDCoord> GetAffectedTiles(int Length, int Width, int PosX, int PosY, int Rotation)
        {
            var x = 0;

            var PointList = new Dictionary<int, ThreeDCoord>();

            if (Length > 1)
            {
                if (Rotation == 0 || Rotation == 4)
                {
                    for (var i = 1; i < Length; i++)
                    {
                        PointList.Add(x++, new ThreeDCoord(PosX, PosY + i, i));

                        for (var j = 1; j < Width; j++)
                        {
                            PointList.Add(x++, new ThreeDCoord(PosX + j, PosY + i, i < j ? j : i));
                        }
                    }
                }
                else if (Rotation == 2 || Rotation == 6)
                {
                    for (var i = 1; i < Length; i++)
                    {
                        PointList.Add(x++, new ThreeDCoord(PosX + i, PosY, i));

                        for (var j = 1; j < Width; j++)
                        {
                            PointList.Add(x++, new ThreeDCoord(PosX + i, PosY + j, i < j ? j : i));
                        }
                    }
                }
            }

            if (Width > 1)
            {
                if (Rotation == 0 || Rotation == 4)
                {
                    for (var i = 1; i < Width; i++)
                    {
                        PointList.Add(x++, new ThreeDCoord(PosX + i, PosY, i));

                        for (var j = 1; j < Length; j++)
                        {
                            PointList.Add(x++, new ThreeDCoord(PosX + i, PosY + j, i < j ? j : i));
                        }
                    }
                }
                else if (Rotation == 2 || Rotation == 6)
                {
                    for (var i = 1; i < Width; i++)
                    {
                        PointList.Add(x++, new ThreeDCoord(PosX, PosY + i, i));

                        for (var j = 1; j < Length; j++)
                        {
                            PointList.Add(x++, new ThreeDCoord(PosX + j, PosY + i, i < j ? j : i));
                        }
                    }
                }
            }

            return PointList;
        }

        public List<Item> GetItemsFromIds(List<int> Input)
        {
            if (Input == null || Input.Count == 0)
            {
                return new List<Item>();
            }

            var Items = new List<Item>();

            lock (Input)
            {
                foreach (var Id in Input.ToList())
                {
                    var Itm = _room.GetRoomItemHandler().GetItem(Id);
                    if (Itm != null && !Items.Contains(Itm))
                    {
                        Items.Add(Itm);
                    }
                }
            }

            return Items.ToList();
        }

        public List<Item> GetRoomItemForSquare(int pX, int pY, double minZ)
        {
            var itemsToReturn = new List<Item>();


            var coord = new Point(pX, pY);
            if (_coordinatedItems.ContainsKey(coord))
            {
                var itemsFromSquare = GetItemsFromIds(_coordinatedItems[coord]);

                foreach (var item in itemsFromSquare)
                {
                    if (item.GetZ > minZ)
                    {
                        if (item.GetX == pX && item.GetY == pY)
                        {
                            itemsToReturn.Add(item);
                        }
                    }
                }
            }

            return itemsToReturn;
        }

        public List<Item> GetRoomItemForSquare(int pX, int pY)
        {
            var coord = new Point(pX, pY);
            //List<RoomItem> itemsFromSquare = new List<RoomItem>();
            var itemsToReturn = new List<Item>();

            if (_coordinatedItems.ContainsKey(coord))
            {
                var itemsFromSquare = GetItemsFromIds(_coordinatedItems[coord]);

                foreach (var item in itemsFromSquare)
                {
                    if (item.Coordinate.X == coord.X && item.Coordinate.Y == coord.Y)
                    {
                        itemsToReturn.Add(item);
                    }
                }
            }

            return itemsToReturn;
        }

        public List<Item> GetAllRoomItemForSquare(int x, int y)
        {
            var coord = new Point(x, y);

            var items = new List<Item>();


            if (_coordinatedItems.TryGetValue(coord, out var Ids))
            {
                items = GetItemsFromIds(Ids);
            }
            else
            {
                items = new List<Item>();
            }

            return items;
        }

        public bool SquareHasUsers(int X, int Y)
        {
            return MapGotUser(new Point(X, Y));
        }


        public static bool TilesTouching(int X1, int Y1, int X2, int Y2)
        {
            if (!(Math.Abs(X1 - X2) > 1 || Math.Abs(Y1 - Y2) > 1))
            {
                return true;
            }

            if (X1 == X2 && Y1 == Y2)
            {
                return true;
            }

            return false;
        }

        public static int TileDistance(int X1, int Y1, int X2, int Y2)
        {
            return Math.Abs(X1 - X2) + Math.Abs(Y1 - Y2);
        }

        public DynamicRoomModel Model { get; private set; }

        public RoomModel StaticModel { get; private set; }

        public byte[,] EffectMap { get; private set; }

        public byte[,] GameMap { get; private set; }

        public void Dispose()
        {
            _userMap.Clear();
            Model.Destroy();
            _coordinatedItems.Clear();

            Array.Clear(GameMap, 0, GameMap.Length);
            Array.Clear(EffectMap, 0, EffectMap.Length);
            Array.Clear(_itemHeightMap, 0, _itemHeightMap.Length);

            _userMap = null;
            GameMap = null;
            EffectMap = null;
            _itemHeightMap = null;
            _coordinatedItems = null;

            Model = null;
            _room = null;
            StaticModel = null;
        }
    }
}