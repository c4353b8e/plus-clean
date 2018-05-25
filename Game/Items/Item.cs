namespace Plus.Game.Items
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Linq;
    using Communication.Packets.Outgoing.Rooms.Engine;
    using Communication.Packets.Outgoing.Rooms.Notifications;
    using Core.Logging;
    using Interactor;
    using Rooms;
    using Rooms.Games.Freeze;
    using Rooms.Games.Teams;
    using Rooms.PathFinding;
    using Users.Authenticator;
    using Utilities;

    public class Item
    {
        private static readonly ILogger Logger = new Logger<Item>();

        public int Id;
        public int BaseItem;
        public string ExtraData;
        public string Figure;
        public string Gender;
        public int GroupId;
        public int InteractingUser;
        public int InteractingUser2;
        public int LimitedNo;
        public int LimitedTot;
        public bool MagicRemove = false;
        public int RoomId;
        public int Rotation;
        public int UpdateCounter;
        public int UserID;
        public string Username;
        public int interactingBallUser;
        public byte interactionCount;
        public byte interactionCountHelper;


        public Team team;
        public bool pendingReset = false;
        public FreezePowerUp freezePowerUp;


        public int value;
        public string wallCoord;
        private bool updateNeeded;

        private Room _room;
        private static readonly Random _random = new Random();

        public Item(int Id, int RoomId, int BaseItem, string ExtraData, int X, int Y, double Z, int Rot, int Userid, int Group, int limitedNumber, int limitedStack, string wallCoord, Room Room = null)
        {
            if (Program.GameContext.GetItemManager().GetItem(BaseItem, out var data))
            {
                this.Id = Id;
                this.RoomId = RoomId;
                _room = Room;
                Data = data;
                this.BaseItem = BaseItem;
                this.ExtraData = ExtraData;
                GroupId = Group;

                GetX = X;
                GetY = Y;
                if (!double.IsInfinity(Z))
                {
                    GetZ = Z;
                }

                Rotation = Rot;
                UpdateNeeded = false;
                UpdateCounter = 0;
                InteractingUser = 0;
                InteractingUser2 = 0;
                interactingBallUser = 0;
                interactionCount = 0;
                value = 0;

                UserID = Userid;
                Username = HabboFactory.GetUsernameById(Userid);


                LimitedNo = limitedNumber;
                LimitedTot = limitedStack;

                switch (GetBaseItem().InteractionType)
                {
                    case InteractionType.TELEPORT:
                        RequestUpdate(0, true);
                        break;

                    case InteractionType.HOPPER:
                        RequestUpdate(0, true);
                        break;

                    case InteractionType.ROLLER:
                        IsRoller = true;
                        if (RoomId > 0)
                        {
                            GetRoom().GetRoomItemHandler().GotRollers = true;
                        }
                        break;

                    case InteractionType.banzaiscoreblue:
                    case InteractionType.footballcounterblue:
                    case InteractionType.banzaigateblue:
                    case InteractionType.FREEZE_BLUE_GATE:
                    case InteractionType.freezebluecounter:
                        team = Team.Blue;
                        break;

                    case InteractionType.banzaiscoregreen:
                    case InteractionType.footballcountergreen:
                    case InteractionType.banzaigategreen:
                    case InteractionType.freezegreencounter:
                    case InteractionType.FREEZE_GREEN_GATE:
                        team = Team.Green;
                        break;

                    case InteractionType.banzaiscorered:
                    case InteractionType.footballcounterred:
                    case InteractionType.banzaigatered:
                    case InteractionType.freezeredcounter:
                    case InteractionType.FREEZE_RED_GATE:
                        team = Team.Red;
                        break;

                    case InteractionType.banzaiscoreyellow:
                    case InteractionType.footballcounteryellow:
                    case InteractionType.banzaigateyellow:
                    case InteractionType.freezeyellowcounter:
                    case InteractionType.FREEZE_YELLOW_GATE:
                        team = Team.Yellow;
                        break;

                    case InteractionType.banzaitele:
                        {
                            this.ExtraData = "";
                            break;
                        }
                }

                IsWallItem = GetBaseItem().Type.ToString().ToLower() == "i";
                IsFloorItem = GetBaseItem().Type.ToString().ToLower() == "s";

                if (IsFloorItem)
                {
                    GetAffectedTiles = Gamemap.GetAffectedTiles(GetBaseItem().Length, GetBaseItem().Width, GetX, GetY, Rot);
                }
                else if (IsWallItem)
                {
                    this.wallCoord = wallCoord;
                    IsWallItem = true;
                    IsFloorItem = false;
                    GetAffectedTiles = new Dictionary<int, ThreeDCoord>();
                }
            }
        }

        public ItemData Data { get; set; }

        public Dictionary<int, ThreeDCoord> GetAffectedTiles { get; private set; }

        public int GetX { get; set; }

        public int GetY { get; set; }

        public double GetZ { get; set; }

        public bool UpdateNeeded
        {
            get => updateNeeded;
            set
            {
                if (value && GetRoom() != null)
                {
                    GetRoom().GetRoomItemHandler().QueueRoomItemUpdate(this);
                }

                updateNeeded = value;
            }
        }

        public bool IsRoller { get; }

        public Point Coordinate => new Point(GetX, GetY);

        public List<Point> GetCoords
        {
            get
            {
                var toReturn = new List<Point>
                {
                    Coordinate
                };

                foreach (var tile in GetAffectedTiles.Values)
                {
                    toReturn.Add(new Point(tile.X, tile.Y));
                }

                return toReturn;
            }
        }

        public List<Point> GetSides()
        {
            var sides = new List<Point>
            {
                SquareBehind,
                SquareInFront,
                SquareLeft,
                SquareRight,
                Coordinate
            };
            return sides;
        }

        public double TotalHeight
        {
            get
            {
                var CurHeight = 0.0;

                if (GetBaseItem().AdjustableHeights.Count > 1)
                {
                    if (int.TryParse(ExtraData, out var num2) && GetBaseItem().AdjustableHeights.Count - 1 >= num2)
                    {
                        CurHeight = GetZ + GetBaseItem().AdjustableHeights[num2];
                    }
                }

                if (CurHeight <= 0.0)
                {
                    CurHeight = GetZ + GetBaseItem().Height;
                }

                return CurHeight;
            }
        }

        public bool IsWallItem { get; }

        public bool IsFloorItem { get; }

        public Point SquareInFront
        {
            get
            {
                var Sq = new Point(GetX, GetY);

                if (Rotation == 0)
                {
                    Sq.Y--;
                }
                else if (Rotation == 2)
                {
                    Sq.X++;
                }
                else if (Rotation == 4)
                {
                    Sq.Y++;
                }
                else if (Rotation == 6)
                {
                    Sq.X--;
                }

                return Sq;
            }
        }

        public Point SquareBehind
        {
            get
            {
                var Sq = new Point(GetX, GetY);

                if (Rotation == 0)
                {
                    Sq.Y++;
                }
                else if (Rotation == 2)
                {
                    Sq.X--;
                }
                else if (Rotation == 4)
                {
                    Sq.Y--;
                }
                else if (Rotation == 6)
                {
                    Sq.X++;
                }

                return Sq;
            }
        }

        public Point SquareLeft
        {
            get
            {
                var Sq = new Point(GetX, GetY);

                if (Rotation == 0)
                {
                    Sq.X++;
                }
                else if (Rotation == 2)
                {
                    Sq.Y--;
                }
                else if (Rotation == 4)
                {
                    Sq.X--;
                }
                else if (Rotation == 6)
                {
                    Sq.Y++;
                }

                return Sq;
            }
        }

        public Point SquareRight
        {
            get
            {
                var Sq = new Point(GetX, GetY);

                if (Rotation == 0)
                {
                    Sq.X--;
                }
                else if (Rotation == 2)
                {
                    Sq.Y++;
                }
                else if (Rotation == 4)
                {
                    Sq.X++;
                }
                else if (Rotation == 6)
                {
                    Sq.Y--;
                }
                return Sq;
            }
        }

        public IFurniInteractor Interactor
        {
            get
            {
                if (IsWired)
                {
                    return new InteractorWired();
                }

                switch (GetBaseItem().InteractionType)
                {
                    case InteractionType.GATE:
                        return new InteractorGate();

                    case InteractionType.TELEPORT:
                        return new InteractorTeleport();

                    case InteractionType.HOPPER:
                        return new InteractorHopper();

                    case InteractionType.BOTTLE:
                        return new InteractorSpinningBottle();

                    case InteractionType.DICE:
                        return new InteractorDice();

                    case InteractionType.HABBO_WHEEL:
                        return new InteractorHabboWheel();

                    case InteractionType.LOVE_SHUFFLER:
                        return new InteractorLoveShuffler();

                    case InteractionType.ONE_WAY_GATE:
                        return new InteractorOneWayGate();

                    case InteractionType.ALERT:
                        return new InteractorAlert();

                    case InteractionType.VENDING_MACHINE:
                        return new InteractorVendor();

                    case InteractionType.SCOREBOARD:
                        return new InteractorScoreboard();

                    case InteractionType.PUZZLE_BOX:
                        return new InteractorPuzzleBox();

                    case InteractionType.MANNEQUIN:
                        return new InteractorMannequin();

                    case InteractionType.banzaicounter:
                        return new InteractorBanzaiTimer();

                    case InteractionType.freezetimer:
                        return new InteractorFreezeTimer();

                    case InteractionType.FREEZE_TILE_BLOCK:
                    case InteractionType.FREEZE_TILE:
                        return new InteractorFreezeTile();

                    case InteractionType.footballcounterblue:
                    case InteractionType.footballcountergreen:
                    case InteractionType.footballcounterred:
                    case InteractionType.footballcounteryellow:
                        return new InteractorScoreCounter();

                    case InteractionType.banzaiscoreblue:
                    case InteractionType.banzaiscoregreen:
                    case InteractionType.banzaiscorered:
                    case InteractionType.banzaiscoreyellow:
                        return new InteractorBanzaiScoreCounter();

                    case InteractionType.WF_FLOOR_SWITCH_1:
                    case InteractionType.WF_FLOOR_SWITCH_2:
                        return new InteractorSwitch();

                    case InteractionType.LOVELOCK:
                        return new InteractorLoveLock();

                    case InteractionType.CANNON:
                        return new InteractorCannon();

                    case InteractionType.COUNTER:
                        return new InteractorCounter();

                    case InteractionType.NONE:
                    default:
                        return new InteractorGenericSwitch();
                }
            }
        }

        public bool IsWired
        {
            get
            {
                switch (GetBaseItem().InteractionType)
                {
                    case InteractionType.WIRED_EFFECT:
                    case InteractionType.WIRED_TRIGGER:
                    case InteractionType.WIRED_CONDITION:
                        return true;
                }

                return false;
            }
        }

        public void SetState(int pX, int pY, double pZ, Dictionary<int, ThreeDCoord> Tiles)
        {
            GetX = pX;
            GetY = pY;
            if (!double.IsInfinity(pZ))
            {
                GetZ = pZ;
            }
            GetAffectedTiles = Tiles;
        }

        public void ProcessUpdates()
        {
            try
            {
                UpdateCounter--;

                if (UpdateCounter <= 0)
                {
                    UpdateNeeded = false;
                    UpdateCounter = 0;

                    RoomUser User = null;
                    RoomUser User2 = null;

                    switch (GetBaseItem().InteractionType)
                    {
                        #region Group Gates
                        case InteractionType.GUILD_GATE:
                            {
                                if (ExtraData == "1")
                                {
                                    if (GetRoom().GetRoomUserManager().GetUserForSquare(GetX, GetY) == null)
                                    {
                                        ExtraData = "0";
                                        UpdateState(false, true);
                                    }
                                    else
                                    {
                                        RequestUpdate(2, false);
                                    }
                                }
                                break;
                            }
                        #endregion

                        #region Item Effects
                        case InteractionType.EFFECT:
                            {
                                if (ExtraData == "1")
                                {
                                    if (GetRoom().GetRoomUserManager().GetUserForSquare(GetX, GetY) == null)
                                    {
                                        ExtraData = "0";
                                        UpdateState(false, true);
                                    }
                                    else
                                    {
                                        RequestUpdate(2, false);
                                    }
                                }
                                break;
                            }
                        #endregion

                        #region One way gates
                        case InteractionType.ONE_WAY_GATE:

                            User = null;

                            if (InteractingUser > 0)
                            {
                                User = GetRoom().GetRoomUserManager().GetRoomUserByHabbo(InteractingUser);
                            }

                            if (User != null && User.X == GetX && User.Y == GetY)
                            {
                                ExtraData = "1";

                                User.MoveTo(SquareBehind);
                                User.InteractingGate = false;
                                User.GateId = 0;
                                RequestUpdate(1, false);
                                UpdateState(false, true);
                            }
                            else if (User != null && User.Coordinate == SquareBehind)
                            {
                                User.UnlockWalking();

                                ExtraData = "0";
                                InteractingUser = 0;
                                User.InteractingGate = false;
                                User.GateId = 0;
                                UpdateState(false, true);
                            }
                            else if (ExtraData == "1")
                            {
                                ExtraData = "0";
                                UpdateState(false, true);
                            }

                            if (User == null)
                            {
                                InteractingUser = 0;
                            }

                            break;
                        #endregion

                        #region VIP Gate
                        case InteractionType.GATE_VIP:

                            User = null;


                            if (InteractingUser > 0)
                            {
                                User = GetRoom().GetRoomUserManager().GetRoomUserByHabbo(InteractingUser);
                            }

                            var NewY = 0;
                            var NewX = 0;

                            if (User != null && User.X == GetX && User.Y == GetY)
                            {
                                if (User.RotBody == 4)
                                {
                                    NewY = 1;
                                }
                                else if (User.RotBody == 0)
                                {
                                    NewY = -1;
                                }
                                else if (User.RotBody == 6)
                                {
                                    NewX = -1;
                                }
                                else if (User.RotBody == 2)
                                {
                                    NewX = 1;
                                }


                                User.MoveTo(User.X + NewX, User.Y + NewY);
                                RequestUpdate(1, false);
                            }
                            else if (User != null && (User.Coordinate == SquareBehind || User.Coordinate == SquareInFront))
                            {
                                User.UnlockWalking();

                                ExtraData = "0";
                                InteractingUser = 0;
                                UpdateState(false, true);
                            }
                            else if (ExtraData == "1")
                            {
                                ExtraData = "0";
                                UpdateState(false, true);
                            }

                            if (User == null)
                            {
                                InteractingUser = 0;
                            }

                            break;
                        #endregion

                        #region Hopper
                        case InteractionType.HOPPER:
                            {
                                User = null;
                                User2 = null;
                                var showHopperEffect = false;
                                var keepDoorOpen = false;
                                var Pause = 0;


                                // Do we have a primary user that wants to go somewhere?
                                if (InteractingUser > 0)
                                {
                                    User = GetRoom().GetRoomUserManager().GetRoomUserByHabbo(InteractingUser);

                                    // Is this user okay?
                                    if (User != null)
                                    {
                                        // Is he in the tele?
                                        if (User.Coordinate == Coordinate)
                                        {
                                            //Remove the user from the square
                                            User.AllowOverride = false;
                                            if (User.TeleDelay == 0)
                                            {
                                                var RoomHopId = ItemHopperFinder.GetAHopper(User.RoomId);
                                                var NextHopperId = ItemHopperFinder.GetHopperId(RoomHopId);

                                                if (!User.IsBot && User != null && User.GetClient() != null &&
                                                    User.GetClient().GetHabbo() != null)
                                                {
                                                    User.GetClient().GetHabbo().IsHopping = true;
                                                    User.GetClient().GetHabbo().HopperId = NextHopperId;
                                                    User.GetClient().GetHabbo().PrepareRoom(RoomHopId, "");
                                                    //User.GetClient().SendMessage(new RoomForwardComposer(RoomHopId));
                                                    InteractingUser = 0;
                                                }
                                            }
                                            else
                                            {
                                                User.TeleDelay--;
                                                showHopperEffect = true;
                                            }
                                        }
                                        // Is he in front of the tele?
                                        else if (User.Coordinate == SquareInFront)
                                        {
                                            User.AllowOverride = true;
                                            keepDoorOpen = true;

                                            // Lock his walking. We're taking control over him. Allow overriding so he can get in the tele.
                                            if (User.IsWalking && (User.GoalX != GetX || User.GoalY != GetY))
                                            {
                                                User.ClearMovement(true);
                                            }

                                            User.CanWalk = false;
                                            User.AllowOverride = true;

                                            // Move into the tele
                                            User.MoveTo(Coordinate.X, Coordinate.Y, true);
                                        }
                                        // Not even near, do nothing and move on for the next user.
                                        else
                                        {
                                            InteractingUser = 0;
                                        }
                                    }
                                    else
                                    {
                                        // Invalid user, do nothing and move on for the next user. 
                                        InteractingUser = 0;
                                    }
                                }

                                if (InteractingUser2 > 0)
                                {
                                    User2 = GetRoom().GetRoomUserManager().GetRoomUserByHabbo(InteractingUser2);

                                    // Is this user okay?
                                    if (User2 != null)
                                    {
                                        // If so, open the door, unlock the user's walking, and try to push him out in the right direction. We're done with him!
                                        keepDoorOpen = true;
                                        User2.UnlockWalking();
                                        User2.MoveTo(SquareInFront);
                                    }

                                    // This is a one time thing, whether the user's valid or not.
                                    InteractingUser2 = 0;
                                }

                                // Set the new item state, by priority
                                if (keepDoorOpen)
                                {
                                    if (ExtraData != "1")
                                    {
                                        ExtraData = "1";
                                        UpdateState(false, true);
                                    }
                                }
                                else if (showHopperEffect)
                                {
                                    if (ExtraData != "2")
                                    {
                                        ExtraData = "2";
                                        UpdateState(false, true);
                                    }
                                }
                                else
                                {
                                    if (ExtraData != "0")
                                    {
                                        if (Pause == 0)
                                        {
                                            ExtraData = "0";
                                            UpdateState(false, true);
                                            Pause = 2;
                                        }
                                        else
                                        {
                                            Pause--;
                                        }
                                    }
                                }

                                // We're constantly going!
                                RequestUpdate(1, false);
                                break;
                            }
                        #endregion

                        #region Teleports
                        case InteractionType.TELEPORT:
                            {
                                User = null;
                                User2 = null;

                                var keepDoorOpen = false;
                                var showTeleEffect = false;

                                // Do we have a primary user that wants to go somewhere?
                                if (InteractingUser > 0)
                                {
                                    User = GetRoom().GetRoomUserManager().GetRoomUserByHabbo(InteractingUser);

                                    // Is this user okay?
                                    if (User != null)
                                    {
                                        // Is he in the tele?
                                        if (User.Coordinate == Coordinate)
                                        {
                                            //Remove the user from the square
                                            User.AllowOverride = false;

                                            if (ItemTeleporterFinder.IsTeleLinked(Id, GetRoom()))
                                            {
                                                showTeleEffect = true;

                                                if (true)
                                                {
                                                    // Woop! No more delay.
                                                    var TeleId = ItemTeleporterFinder.GetLinkedTele(Id);
                                                    var RoomId = ItemTeleporterFinder.GetTeleRoomId(TeleId, GetRoom());

                                                    // Do we need to tele to the same room or gtf to another?
                                                    if (RoomId == this.RoomId)
                                                    {
                                                        var Item = GetRoom().GetRoomItemHandler().GetItem(TeleId);

                                                        if (Item == null)
                                                        {
                                                            User.UnlockWalking();
                                                        }
                                                        else
                                                        {
                                                            // Set pos
                                                            User.SetPos(Item.GetX, Item.GetY, Item.GetZ);
                                                            User.SetRot(Item.Rotation, false);

                                                            // Force tele effect update (dirty)
                                                            Item.ExtraData = "2";
                                                            Item.UpdateState(false, true);

                                                            // Set secondary interacting user
                                                            Item.InteractingUser2 = InteractingUser;
                                                            GetRoom().GetGameMap().RemoveUserFromMap(User, new Point(GetX, GetY));

                                                            InteractingUser = 0;
                                                        }
                                                    }
                                                    else
                                                    {
                                                        if (User.TeleDelay == 0)
                                                        {
                                                            // Let's run the teleport delegate to take futher care of this.. WHY DARIO?!
                                                            if (!User.IsBot && User != null && User.GetClient() != null &&
                                                                User.GetClient().GetHabbo() != null)
                                                            {
                                                                User.GetClient().GetHabbo().IsTeleporting = true;
                                                                User.GetClient().GetHabbo().TeleportingRoomID = RoomId;
                                                                User.GetClient().GetHabbo().TeleporterId = TeleId;
                                                                User.GetClient().GetHabbo().PrepareRoom(RoomId, "");
                                                                //User.GetClient().SendMessage(new RoomForwardComposer(RoomId));
                                                                InteractingUser = 0;
                                                            }
                                                        }
                                                        else
                                                        {
                                                            User.TeleDelay--;
                                                            showTeleEffect = true;
                                                        }
                                                        //Program.Game.GetRoomManager().AddTeleAction(new TeleUserData(User.GetClient().GetMessageHandler(), User.GetClient().GetHabbo(), RoomId, TeleId));
                                                    }
                                                    GetRoom().GetGameMap().GenerateMaps();
                                                    // We're done with this tele. We have another one to bother.
                                                }
                                            }
                                            else
                                            {
                                                // This tele is not linked, so let's gtfo.
                                                User.UnlockWalking();
                                                InteractingUser = 0;
                                            }
                                        }
                                        // Is he in front of the tele?
                                        else if (User.Coordinate == SquareInFront)
                                        {
                                                User.AllowOverride = true;
                                                // Open the door
                                                keepDoorOpen = true;

                                                // Lock his walking. We're taking control over him. Allow overriding so he can get in the tele.
                                                if (User.IsWalking && (User.GoalX != GetX || User.GoalY != GetY))
                                                {
                                                    User.ClearMovement(true);
                                                }

                                                User.CanWalk = false;
                                                User.AllowOverride = true;

                                                // Move into the tele
                                                User.MoveTo(Coordinate.X, Coordinate.Y, true);
                                            
                                        }
                                        // Not even near, do nothing and move on for the next user.
                                        else
                                        {
                                            InteractingUser = 0;
                                        }
                                    }
                                    else
                                    {
                                        // Invalid user, do nothing and move on for the next user. 
                                        InteractingUser = 0;
                                    }
                                }

                                // Do we have a secondary user that wants to get out of the tele?
                                if (InteractingUser2 > 0)
                                {
                                    User2 = GetRoom().GetRoomUserManager().GetRoomUserByHabbo(InteractingUser2);

                                    // Is this user okay?
                                    if (User2 != null)
                                    {
                                        // If so, open the door, unlock the user's walking, and try to push him out in the right direction. We're done with him!
                                        keepDoorOpen = true;
                                        User2.UnlockWalking();
                                        User2.MoveTo(SquareInFront);
                                    }

                                    // This is a one time thing, whether the user's valid or not.
                                    InteractingUser2 = 0;
                                }

                                // Set the new item state, by priority
                                if (showTeleEffect)
                                {
                                    if (ExtraData != "2")
                                    {
                                        ExtraData = "2";
                                        UpdateState(false, true);
                                    }
                                }
                                else if (keepDoorOpen)
                                {
                                    if (ExtraData != "1")
                                    {
                                        ExtraData = "1";
                                        UpdateState(false, true);
                                    }
                                }
                                else
                                {
                                    if (ExtraData != "0")
                                    {
                                        ExtraData = "0";
                                        UpdateState(false, true);
                                    }
                                }

                                // We're constantly going!
                                RequestUpdate(1, false);
                                break;
                            }
                        #endregion

                        #region Bottle
                        case InteractionType.BOTTLE:
                            ExtraData = RandomNumber.GenerateNewRandom(0, 7).ToString();
                            UpdateState();
                            break;
                        #endregion

                        #region Dice
                        case InteractionType.DICE:
                            {
                                var numbers = new[] { "1", "2", "3", "4", "5", "6" };
                                if (ExtraData == "-1")
                                {
                                    ExtraData = RandomizeStrings(numbers)[0];
                                }

                                UpdateState();
                            }
                            break;
                        #endregion

                        #region Habbo Wheel
                        case InteractionType.HABBO_WHEEL:
                            ExtraData = RandomNumber.GenerateRandom(1, 10).ToString();
                            UpdateState();
                            break;
                        #endregion

                        #region Love Shuffler
                        case InteractionType.LOVE_SHUFFLER:

                            if (ExtraData == "0")
                            {
                                ExtraData = RandomNumber.GenerateNewRandom(1, 4).ToString();
                                RequestUpdate(20, false);
                            }
                            else if (ExtraData != "-1")
                            {
                                ExtraData = "-1";
                            }

                            UpdateState(false, true);
                            break;
                        #endregion

                        #region Alert
                        case InteractionType.ALERT:
                            if (ExtraData == "1")
                            {
                                ExtraData = "0";
                                UpdateState(false, true);
                            }
                            break;
                        #endregion

                        #region Vending Machine
                        case InteractionType.VENDING_MACHINE:

                            if (ExtraData == "1")
                            {
                                User = GetRoom().GetRoomUserManager().GetRoomUserByHabbo(InteractingUser);
                                if (User == null)
                                {
                                    break;
                                }

                                User.UnlockWalking();
                                if (GetBaseItem().VendingIds.Count > 0)
                                {
                                    var randomDrink = GetBaseItem().VendingIds[RandomNumber.GenerateRandom(0, GetBaseItem().VendingIds.Count - 1)];
                                    User.CarryItem(randomDrink);
                                }


                                InteractingUser = 0;
                                ExtraData = "0";

                                UpdateState(false, true);
                            }
                            break;
                        #endregion

                        #region Scoreboard
                        case InteractionType.SCOREBOARD:
                            {
                                if (string.IsNullOrEmpty(ExtraData))
                                {
                                    break;
                                }


                                var seconds = 0;

                                try
                                {
                                    seconds = int.Parse(ExtraData);
                                }
                                catch
                                {
                                }

                                if (seconds > 0)
                                {
                                    if (interactionCountHelper == 1)
                                    {
                                        seconds--;
                                        interactionCountHelper = 0;

                                        ExtraData = seconds.ToString();
                                        UpdateState();
                                    }
                                    else
                                    {
                                        interactionCountHelper++;
                                    }

                                    UpdateCounter = 1;
                                }
                                else
                                {
                                    UpdateCounter = 0;
                                }

                                break;
                            }
                        #endregion

                        #region Banzai Counter
                        case InteractionType.banzaicounter:
                            {
                                if (string.IsNullOrEmpty(ExtraData))
                                {
                                    break;
                                }

                                var seconds = 0;

                                try
                                {
                                    seconds = int.Parse(ExtraData);
                                }
                                catch
                                {
                                }

                                if (seconds > 0)
                                {
                                    if (interactionCountHelper == 1)
                                    {
                                        seconds--;
                                        interactionCountHelper = 0;

                                        if (GetRoom().GetBanzai().IsBanzaiActive)
                                        {
                                            ExtraData = seconds.ToString();
                                            UpdateState();
                                        }
                                        else
                                        {
                                            break;
                                        }
                                    }
                                    else
                                    {
                                        interactionCountHelper++;
                                    }

                                    UpdateCounter = 1;
                                }
                                else
                                {
                                    UpdateCounter = 0;
                                    GetRoom().GetBanzai().BanzaiEnd();
                                }

                                break;
                            }
                        #endregion

                        #region Banzai Tele
                        case InteractionType.banzaitele:
                            {
                                ExtraData = string.Empty;
                                UpdateState();
                                break;
                            }
                        #endregion

                        #region Banzai Floor
                        case InteractionType.banzaifloor:
                            {
                                if (value == 3)
                                {
                                    if (interactionCountHelper == 1)
                                    {
                                        interactionCountHelper = 0;

                                        switch (team)
                                        {
                                            case Team.Blue:
                                                {
                                                    ExtraData = "11";
                                                    break;
                                                }

                                            case Team.Green:
                                                {
                                                    ExtraData = "8";
                                                    break;
                                                }

                                            case Team.Red:
                                                {
                                                    ExtraData = "5";
                                                    break;
                                                }

                                            case Team.Yellow:
                                                {
                                                    ExtraData = "14";
                                                    break;
                                                }
                                        }
                                    }
                                    else
                                    {
                                        ExtraData = "";
                                        interactionCountHelper++;
                                    }

                                    UpdateState();

                                    interactionCount++;

                                    if (interactionCount < 16)
                                    {
                                        UpdateCounter = 1;
                                    }
                                    else
                                    {
                                        UpdateCounter = 0;
                                    }
                                }
                                break;
                            }
                        #endregion

                        #region Banzai Puck
                        case InteractionType.banzaipuck:
                            {
                                if (interactionCount > 4)
                                {
                                    interactionCount++;
                                    UpdateCounter = 1;
                                }
                                else
                                {
                                    interactionCount = 0;
                                    UpdateCounter = 0;
                                }

                                break;
                            }
                        #endregion

                        #region Freeze Tile
                        case InteractionType.FREEZE_TILE:
                            {
                                if (InteractingUser > 0)
                                {
                                    ExtraData = "11000";
                                    UpdateState(false, true);
                                    GetRoom().GetFreeze().onFreezeTiles(this, freezePowerUp);
                                    InteractingUser = 0;
                                    interactionCountHelper = 0;
                                }
                                break;
                            }
                        #endregion

                        #region Football Counter
                        case InteractionType.COUNTER:
                            {
                                if (string.IsNullOrEmpty(ExtraData))
                                {
                                    break;
                                }

                                var seconds = 0;

                                try
                                {
                                    seconds = int.Parse(ExtraData);
                                }
                                catch
                                {
                                }

                                if (seconds > 0)
                                {
                                    if (interactionCountHelper == 1)
                                    {
                                        seconds--;
                                        interactionCountHelper = 0;
                                        if (GetRoom().GetSoccer().GameIsStarted)
                                        {
                                            ExtraData = seconds.ToString();
                                            UpdateState();
                                        }
                                        else
                                        {
                                            break;
                                        }
                                    }
                                    else
                                    {
                                        interactionCountHelper++;
                                    }

                                    UpdateCounter = 1;
                                }
                                else
                                {
                                    UpdateNeeded = false;
                                    GetRoom().GetSoccer().StopGame();
                                }

                                break;
                            }
                        #endregion

                        #region Freeze Timer
                        case InteractionType.freezetimer:
                            {
                                if (string.IsNullOrEmpty(ExtraData))
                                {
                                    break;
                                }

                                var seconds = 0;

                                try
                                {
                                    seconds = int.Parse(ExtraData);
                                }
                                catch
                                {
                                }

                                if (seconds > 0)
                                {
                                    if (interactionCountHelper == 1)
                                    {
                                        seconds--;
                                        interactionCountHelper = 0;
                                        if (GetRoom().GetFreeze().GameIsStarted)
                                        {
                                            ExtraData = seconds.ToString();
                                            UpdateState();
                                        }
                                        else
                                        {
                                            break;
                                        }
                                    }
                                    else
                                    {
                                        interactionCountHelper++;
                                    }

                                    UpdateCounter = 1;
                                }
                                else
                                {
                                    UpdateNeeded = false;
                                    GetRoom().GetFreeze().StopGame();
                                }

                                break;
                            }
                        #endregion

                        #region Pressure Pad
                        case InteractionType.PRESSURE_PAD:
                            {
                                ExtraData = "1";
                                UpdateState();
                                break;
                            }
                        #endregion

                        #region Wired
                        case InteractionType.WIRED_EFFECT:
                        case InteractionType.WIRED_TRIGGER:
                        case InteractionType.WIRED_CONDITION:
                            {
                                if (ExtraData == "1")
                                {
                                    ExtraData = "0";
                                    UpdateState(false, true);
                                }
                            }
                            break;
                        #endregion

                        #region Cannon
                        case InteractionType.CANNON:
                            {
                                if (ExtraData != "1")
                                {
                                    break;
                                }

                                #region Target Calculation
                                var TargetStart = Coordinate;
                                var TargetSquares = new List<Point>();
                                switch (Rotation)
                                {
                                    case 0:
                                        {
                                            TargetStart = new Point(GetX - 1, GetY);

                                            if (!TargetSquares.Contains(TargetStart))
                                            {
                                                TargetSquares.Add(TargetStart);
                                            }

                                            for (var I = 1; I <= 3; I++)
                                            {
                                                var TargetSquare = new Point(TargetStart.X - I, TargetStart.Y);

                                                if (!TargetSquares.Contains(TargetSquare))
                                                {
                                                    TargetSquares.Add(TargetSquare);
                                                }
                                            }

                                            break;
                                        }

                                    case 2:
                                        {
                                            TargetStart = new Point(GetX, GetY - 1);

                                            if (!TargetSquares.Contains(TargetStart))
                                            {
                                                TargetSquares.Add(TargetStart);
                                            }

                                            for (var I = 1; I <= 3; I++)
                                            {
                                                var TargetSquare = new Point(TargetStart.X, TargetStart.Y - I);

                                                if (!TargetSquares.Contains(TargetSquare))
                                                {
                                                    TargetSquares.Add(TargetSquare);
                                                }
                                            }

                                            break;
                                        }

                                    case 4:
                                        {
                                            TargetStart = new Point(GetX + 2, GetY);

                                            if (!TargetSquares.Contains(TargetStart))
                                            {
                                                TargetSquares.Add(TargetStart);
                                            }

                                            for (var I = 1; I <= 3; I++)
                                            {
                                                var TargetSquare = new Point(TargetStart.X + I, TargetStart.Y);

                                                if (!TargetSquares.Contains(TargetSquare))
                                                {
                                                    TargetSquares.Add(TargetSquare);
                                                }
                                            }

                                            break;
                                        }

                                    case 6:
                                        {
                                            TargetStart = new Point(GetX, GetY + 2);


                                            if (!TargetSquares.Contains(TargetStart))
                                            {
                                                TargetSquares.Add(TargetStart);
                                            }

                                            for (var I = 1; I <= 3; I++)
                                            {
                                                var TargetSquare = new Point(TargetStart.X, TargetStart.Y + I);

                                                if (!TargetSquares.Contains(TargetSquare))
                                                {
                                                    TargetSquares.Add(TargetSquare);
                                                }
                                            }

                                            break;
                                        }
                                }
                                #endregion

                                if (TargetSquares.Count > 0)
                                {
                                    foreach (var Square in TargetSquares.ToList())
                                    {
                                        var affectedUsers = _room.GetGameMap().GetRoomUsers(Square).ToList();

                                        if (affectedUsers == null || affectedUsers.Count == 0)
                                        {
                                            continue;
                                        }

                                        foreach (var Target in affectedUsers)
                                        {
                                            if (Target == null || Target.IsBot || Target.IsPet)
                                            {
                                                continue;
                                            }

                                            if (Target.GetClient() == null || Target.GetClient().GetHabbo() == null)
                                            {
                                                continue;
                                            }

                                            if (_room.CheckRights(Target.GetClient(), true))
                                            {
                                                continue;
                                            }

                                            Target.ApplyEffect(4);
                                            Target.GetClient().SendPacket(new RoomNotificationComposer("Kicked from room", "You were hit by a cannonball!", "room_kick_cannonball", ""));
                                            Target.ApplyEffect(0);
                                            _room.GetRoomUserManager().RemoveUserFromRoom(Target.GetClient(), true);
                                        }
                                    }
                                }

                                ExtraData = "2";
                                UpdateState(false, true);
                            }
                            break;
                            #endregion
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }
        }

        public static string[] RandomizeStrings(string[] arr)
        {
            var list = new List<KeyValuePair<int, string>>();
            // Add all strings from array
            // Add new random int each time
            foreach (var s in arr)
            {
                list.Add(new KeyValuePair<int, string>(_random.Next(), s));
            }
            // Sort the list by the random number
            var sorted = from item in list
                         orderby item.Key
                         select item;
            // Allocate new string array
            var result = new string[arr.Length];
            // Copy values to array
            var index = 0;
            foreach (var pair in sorted)
            {
                result[index] = pair.Value;
                index++;
            }
            // Return copied array
            return result;
        }

        public void RequestUpdate(int Cycles, bool setUpdate)
        {
            UpdateCounter = Cycles;
            if (setUpdate)
            {
                UpdateNeeded = true;
            }
        }

        public void UpdateState()
        {
            UpdateState(true, true);
        }

        public void UpdateState(bool inDb, bool inRoom)
        {
            if (GetRoom() == null)
            {
                return;
            }

            if (inDb)
            {
                GetRoom().GetRoomItemHandler().UpdateItem(this);
            }

            if (inRoom)
            {
                if (IsFloorItem)
                {
                    GetRoom().SendPacket(new ObjectUpdateComposer(this, GetRoom().OwnerId));
                }
                else
                {
                    GetRoom().SendPacket(new ItemUpdateComposer(this, GetRoom().OwnerId));
                }
            }
        }

        public void ResetBaseItem()
        {
            Data = null;
            Data = GetBaseItem();
        }

        public ItemData GetBaseItem()
        {
            if (Data == null)
            {
                if (Program.GameContext.GetItemManager().GetItem(BaseItem, out var I))
                {
                    Data = I;
                }
            }

            return Data;
        }

        public Room GetRoom()
        {
            if (_room != null)
            {
                return _room;
            }

            if (Program.GameContext.GetRoomManager().TryGetRoom(RoomId, out var Room))
            {
                return Room;
            }

            return null;
        }

        public void UserFurniCollision(RoomUser user)
        {
            if (user == null || user.GetClient() == null || user.GetClient().GetHabbo() == null)
            {
                return;
            }

            GetRoom().GetWired().TriggerEvent(Wired.WiredBoxType.TriggerUserFurniCollision, user.GetClient().GetHabbo(), this);
        }

        public void UserWalksOnFurni(RoomUser user)
        {
            if (user?.GetClient() == null || user.GetClient().GetHabbo() == null)
            {
                return;
            }

            if (GetBaseItem().InteractionType == InteractionType.TENT || GetBaseItem().InteractionType == InteractionType.TENT_SMALL)
            {
                GetRoom().AddUserToTent(Id, user);
            }

            GetRoom().GetWired().TriggerEvent(Wired.WiredBoxType.TriggerWalkOnFurni, user.GetClient().GetHabbo(), this);
            user.LastItem = this;
        }

        public void UserWalksOffFurni(RoomUser user)
        {
            if (user == null || user.GetClient() == null || user.GetClient().GetHabbo() == null)
            {
                return;
            }

            if (GetBaseItem().InteractionType == InteractionType.TENT || GetBaseItem().InteractionType == InteractionType.TENT_SMALL)
            {
                GetRoom().RemoveUserFromTent(Id, user);
            }

            GetRoom().GetWired().TriggerEvent(Wired.WiredBoxType.TriggerWalkOffFurni, user.GetClient().GetHabbo(), this);
        }

        public void Destroy()
        {
            _room = null;
            Data = null;
            GetAffectedTiles.Clear();
        }
    }
}