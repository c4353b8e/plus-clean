namespace Plus.Game.Rooms
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using System.Threading.Tasks;
    using AI;
    using AI.Speech;
    using Communication.Interfaces;
    using Communication.Packets.Outgoing;
    using Communication.Packets.Outgoing.Rooms.Avatar;
    using Communication.Packets.Outgoing.Rooms.Engine;
    using Communication.Packets.Outgoing.Rooms.Session;
    using Core.Logging;
    using Games;
    using Games.Banzai;
    using Games.Football;
    using Games.Freeze;
    using Games.Teams;
    using Instance;
    using Items;
    using Items.Data.Moodlight;
    using Items.Data.Toner;
    using Players;
    using Utilities;

    public class Room : RoomData
    {
        private static readonly ILogger Logger = new Logger<Room>();

        public bool isCrashed;
        public bool mDisposed;
        public bool RoomMuted;
        public DateTime lastTimerReset;
        public DateTime lastRegeneration;

        public Task ProcessTask;
        public ArrayList ActiveTrades;

        public TonerData TonerData;
        public MoodlightData MoodlightData;

        public Dictionary<int, double> MutedUsers;

        private readonly Dictionary<int, List<RoomUser>> Tents;

        public List<int> UsersWithRights;
        private GameManager _gameManager;
        private Freeze _freeze;
        private Soccer _soccer;
        private BattleBanzai _banzai;

        private Gamemap _gamemap;
        private GameItemHandler _gameItemHandler;
        
        public TeamManager teambanzai;
        public TeamManager teamfreeze;

        private RoomUserManager _roomUserManager;
        private RoomItemHandling _roomItemHandling;

        private readonly FilterComponent _filterComponent;
        private readonly WiredComponent _wiredComponent;
        private readonly BansComponent _bansComponent;
        private readonly TradingComponent _tradingComponent;

        public int IsLagging { get; set; }
        public bool Unloaded { get; set; }
        public int IdleTime { get; set; }

        public Room(RoomData data)
         : base(data)
        {
            IsLagging = 0;
            Unloaded = false;
            IdleTime = 0;

            RoomMuted = false;

            MutedUsers = new Dictionary<int, double>();
            Tents = new Dictionary<int, List<RoomUser>>();

            _gamemap = new Gamemap(this, data.Model);
            _roomItemHandling = new RoomItemHandling(this);

            _roomUserManager = new RoomUserManager(this);
            _filterComponent = new FilterComponent(this);
            _wiredComponent = new WiredComponent(this);
            _bansComponent = new BansComponent(this);
            _tradingComponent = new TradingComponent(this);

            GetRoomItemHandler().LoadFurniture();
            GetGameMap().GenerateMaps();

            LoadPromotions();
            LoadRights();
            LoadFilter();
            InitBots();
            InitPets();

            lastRegeneration = DateTime.Now;
        }

        public List<string> WordFilterList { get; set; }

        public int UserCount => _roomUserManager.GetRoomUsers().Count;

        public int RoomId => Id;

        public bool CanTradeInRoom => true;

        public Gamemap GetGameMap()
        {
            return _gamemap;
        }

        public RoomItemHandling GetRoomItemHandler()
        {
            return _roomItemHandling ?? (_roomItemHandling = new RoomItemHandling(this));
        }

        public RoomUserManager GetRoomUserManager()
        {
            return _roomUserManager;
        }

        public Soccer GetSoccer()
        {
            return _soccer ?? (_soccer = new Soccer(this));
        }

        public TeamManager GetTeamManagerForBanzai()
        {
            return teambanzai ?? (teambanzai = TeamManager.CreateTeam("banzai"));
        }

        public TeamManager GetTeamManagerForFreeze()
        {
            return teamfreeze ?? (teamfreeze = TeamManager.CreateTeam("freeze"));
        }

        public BattleBanzai GetBanzai()
        {
            return _banzai ?? (_banzai = new BattleBanzai(this));
        }

        public Freeze GetFreeze()
        {
            return _freeze ?? (_freeze = new Freeze(this));
        }

        public GameManager GetGameManager()
        {
            return _gameManager ?? (_gameManager = new GameManager(this));
        }

        public GameItemHandler GetGameItemHandler()
        {
            return _gameItemHandler ?? (_gameItemHandler = new GameItemHandler(this));
        }

        public bool GotSoccer()
        {
            return _soccer != null;
        }

        public bool GotBanzai()
        {
            return _banzai != null;
        }

        public bool GotFreeze()
        {
            return _freeze != null;
        }

        public void ClearTags()
        {
            Tags.Clear();
        }

        public void AddTagRange(List<string> tags)
        {
            Tags.AddRange(tags);
        }

        public void InitBots()
        {
            using (var dbClient = Program.DatabaseManager.GetQueryReactor())
            {
                dbClient.SetQuery("SELECT `id`,`room_id`,`name`,`motto`,`look`,`x`,`y`,`z`,`rotation`,`gender`,`user_id`,`ai_type`,`walk_mode`,`automatic_chat`,`speaking_interval`,`mix_sentences`,`chat_bubble` FROM `bots` WHERE `room_id` = '" + RoomId + "' AND `ai_type` != 'pet'");
                var Data = dbClient.GetTable();
                if (Data == null)
                {
                    return;
                }

                foreach (DataRow Bot in Data.Rows)
                {
                    dbClient.SetQuery("SELECT `text` FROM `bots_speech` WHERE `bot_id` = '" + Convert.ToInt32(Bot["id"]) + "'");
                    var BotSpeech = dbClient.GetTable();

                    var Speeches = new List<RandomSpeech>();

                    foreach (DataRow Speech in BotSpeech.Rows)
                    {
                        Speeches.Add(new RandomSpeech(Convert.ToString(Speech["text"]), Convert.ToInt32(Bot["id"])));
                    }

                    _roomUserManager.DeployBot(new RoomBot(Convert.ToInt32(Bot["id"]), Convert.ToInt32(Bot["room_id"]), Convert.ToString(Bot["ai_type"]), Convert.ToString(Bot["walk_mode"]), Convert.ToString(Bot["name"]), Convert.ToString(Bot["motto"]), Convert.ToString(Bot["look"]), int.Parse(Bot["x"].ToString()), int.Parse(Bot["y"].ToString()), int.Parse(Bot["z"].ToString()), int.Parse(Bot["rotation"].ToString()), 0, 0, 0, 0, ref Speeches, "M", 0, Convert.ToInt32(Bot["user_id"].ToString()), Convert.ToBoolean(Bot["automatic_chat"]), Convert.ToInt32(Bot["speaking_interval"]), Bot["mix_sentences"].ToString() == "1", Convert.ToInt32(Bot["chat_bubble"])), null);
                }
            }
        }

        public void InitPets()
        {
            using (var dbClient = Program.DatabaseManager.GetQueryReactor())
            {
                dbClient.SetQuery("SELECT `id`,`user_id`,`room_id`,`name`,`x`,`y`,`z` FROM `bots` WHERE `room_id` = '" + RoomId + "' AND `ai_type` = 'pet'");
                var Data = dbClient.GetTable();

                if (Data == null)
                {
                    return;
                }

                foreach (DataRow Row in Data.Rows)
                {
                    dbClient.SetQuery("SELECT `type`,`race`,`color`,`experience`,`energy`,`nutrition`,`respect`,`createstamp`,`have_saddle`,`anyone_ride`,`hairdye`,`pethair`,`gnome_clothing` FROM `bots_petdata` WHERE `id` = '" + Row[0] + "' LIMIT 1");
                    var mRow = dbClient.GetRow();
                    if (mRow == null)
                    {
                        continue;
                    }

                    var Pet = new Pet(Convert.ToInt32(Row["id"]), Convert.ToInt32(Row["user_id"]), Convert.ToInt32(Row["room_id"]), Convert.ToString(Row["name"]), Convert.ToInt32(mRow["type"]), Convert.ToString(mRow["race"]),
                        Convert.ToString(mRow["color"]), Convert.ToInt32(mRow["experience"]), Convert.ToInt32(mRow["energy"]), Convert.ToInt32(mRow["nutrition"]), Convert.ToInt32(mRow["respect"]), Convert.ToDouble(mRow["createstamp"]), Convert.ToInt32(Row["x"]), Convert.ToInt32(Row["y"]),
                        Convert.ToDouble(Row["z"]), Convert.ToInt32(mRow["have_saddle"]), Convert.ToInt32(mRow["anyone_ride"]), Convert.ToInt32(mRow["hairdye"]), Convert.ToInt32(mRow["pethair"]), Convert.ToString(mRow["gnome_clothing"]));

                    var RndSpeechList = new List<RandomSpeech>();

                    _roomUserManager.DeployBot(new RoomBot(Pet.PetId, RoomId, "pet", "freeroam", Pet.Name, "", Pet.Look, Pet.X, Pet.Y, Convert.ToInt32(Pet.Z), 0, 0, 0, 0, 0, ref RndSpeechList, "", 0, Pet.OwnerId, false, 0, false, 0), Pet);
                }
            }
        }

        public FilterComponent GetFilter()
        {
            return _filterComponent;
        }

        public WiredComponent GetWired()
        {
            return _wiredComponent;
        }

        public BansComponent GetBans()
        {
            return _bansComponent;
        }

        public TradingComponent GetTrading()
        {
            return _tradingComponent;
        }

        public void LoadRights()
        {
            UsersWithRights = new List<int>();
            if (Group != null)
            {
                return;
            }

            DataTable Data = null;

            using (var dbClient = Program.DatabaseManager.GetQueryReactor())
            {
                dbClient.SetQuery("SELECT room_rights.user_id FROM room_rights WHERE room_id = @roomid");
                dbClient.AddParameter("roomid", Id);
                Data = dbClient.GetTable();
            }

            if (Data != null)
            {
                foreach (DataRow Row in Data.Rows)
                {
                    UsersWithRights.Add(Convert.ToInt32(Row["user_id"]));
                }
            }
        }

        private void LoadFilter()
        {
            WordFilterList = new List<string>();

            DataTable Data = null;
            using (var dbClient = Program.DatabaseManager.GetQueryReactor())
            {
                dbClient.SetQuery("SELECT * FROM `room_filter` WHERE `room_id` = @roomid;");
                dbClient.AddParameter("roomid", Id);
                Data = dbClient.GetTable();
            }

            if (Data == null)
            {
                return;
            }

            foreach (DataRow Row in Data.Rows)
            {
                WordFilterList.Add(Convert.ToString(Row["word"]));
            }
        }

        public bool CheckRights(Player Session)
        {
            return CheckRights(Session, false);
        }

        public bool CheckRights(Player Session, bool RequireOwnership, bool CheckForGroups = false)
        {
            try
            {
                if (Session == null || Session.GetHabbo() == null)
                {
                    return false;
                }

                if (Session.GetHabbo().Username == OwnerName && Type == "private")
                {
                    return true;
                }

                if (Session.GetHabbo().GetPermissions().HasRight("room_any_owner"))
                {
                    return true;
                }

                if (!RequireOwnership && Type == "private")
                {
                    if (Session.GetHabbo().GetPermissions().HasRight("room_any_rights"))
                    {
                        return true;
                    }

                    if (UsersWithRights.Contains(Session.GetHabbo().Id))
                    {
                        return true;
                    }
                }

                if (CheckForGroups && Type == "private")
                {
                    if (Group == null)
                    {
                        return false;
                    }

                    if (Group.IsAdmin(Session.GetHabbo().Id))
                    {
                        return true;
                    }

                    if (Group.AdminOnlyDeco == 0)
                    {
                        if (Group.IsAdmin(Session.GetHabbo().Id))
                        {
                            return true;
                        }
                    }
                }
            }
            catch (Exception e) { Logger.Error(e); }
            return false;
        }

        public void OnUserShoot(RoomUser User, Item Ball)
        {
            Func<Item, bool> predicate = null;
            string Key = null;
            foreach (var item in GetRoomItemHandler().GetFurniObjects(Ball.GetX, Ball.GetY).ToList())
            {
                if (item.GetBaseItem().ItemName.StartsWith("fball_goal_"))
                {
                    Key = item.GetBaseItem().ItemName.Split('_')[2];
                    User.UnIdle();
                    User.DanceId = 0;


                    Program.GameContext.GetAchievementManager().ProgressAchievement(User.GetClient(), "ACH_FootballGoalScored", 1);

                    SendPacket(new ActionComposer(User.VirtualId, 1));
                }
            }

            if (Key != null)
            {
                if (predicate == null)
                {
                    predicate = p => p.GetBaseItem().ItemName == "fball_score_" + Key;
                }

                foreach (var item2 in GetRoomItemHandler().GetFloor.Where(predicate).ToList())
                {
                    if (item2.GetBaseItem().ItemName == "fball_score_" + Key)
                    {
                        if (!string.IsNullOrEmpty(item2.ExtraData))
                        {
                            item2.ExtraData = (Convert.ToInt32(item2.ExtraData) + 1).ToString();
                        }
                        else
                        {
                            item2.ExtraData = "1";
                        }

                        item2.UpdateState();
                    }
                }
            }
        }

        public void ProcessRoom()
        {
            if (isCrashed || mDisposed)
            {
                return;
            }

            try
            {
                if (GetRoomUserManager().GetRoomUsers().Count == 0)
                {
                    IdleTime++;
                }
                else if (IdleTime > 0)
                {
                    IdleTime = 0;
                }

                if (HasActivePromotion && Promotion.HasExpired)
                {
                    EndPromotion();
                }

                if (IdleTime >= 60 && !HasActivePromotion)
                {
                    Program.GameContext.GetRoomManager().UnloadRoom(Id);
                    return;
                }

                try { GetRoomItemHandler().OnCycle(); }
                catch (Exception e)
                {
                    Logger.Error(e);
                }

                try { GetRoomUserManager().OnCycle(); }
                catch (Exception e)
                {
                    Logger.Error(e);
                }

                #region Status Updates
                try
                {
                    GetRoomUserManager().SerializeStatusUpdates();
                }
                catch (Exception e)
                {
                    Logger.Error(e);
                }
                #endregion

                #region Game Item Cycle
                try
                {
                    if (_gameItemHandler != null)
                    {
                        _gameItemHandler.OnCycle();
                    }
                }
                catch (Exception e)
                {
                    Logger.Error(e);
                }
                #endregion

                try { GetWired().OnCycle(); }
                catch (Exception e)
                {
                    Logger.Error(e);
                }

            }
            catch (Exception e)
            {
                Logger.Error(e);
                OnRoomCrash(e);
            }
        }

        private void OnRoomCrash(Exception e)
        {
            try
            {
                foreach (var user in _roomUserManager.GetRoomUsers().ToList())
                {
                    if (user == null || user.GetClient() == null)
                    {
                        continue;
                    }

                    user.GetClient().SendNotification("Sorry, it appears that room has crashed!");//Unhandled exception in room: " + e);

                    try
                    {
                        GetRoomUserManager().RemoveUserFromRoom(user.GetClient(), true, false);
                    }
                    catch (Exception e2)
                    {
                        Logger.Error(e2); }
                }
            }
            catch (Exception e3)
            {
                Logger.Error(e3);
            }

            isCrashed = true;
            Program.GameContext.GetRoomManager().UnloadRoom(Id);
        }


        public bool CheckMute(Player session)
        {
            if (MutedUsers.ContainsKey(session.GetHabbo().Id))
            {
                if (MutedUsers[session.GetHabbo().Id] < UnixUtilities.GetNow())
                {
                    MutedUsers.Remove(session.GetHabbo().Id);
                }
                else
                {
                    return true;
                }
            }

            if (session.GetHabbo().TimeMuted > 0 || RoomMuted && session.GetHabbo().Username != OwnerName)
            {
                return true;
            }

            return false;
        }

        public void SendObjects(Player session)
        {
            session.SendPacket(new HeightMapComposer(GetGameMap().Model.Heightmap));
            session.SendPacket(new FloorHeightMapComposer(GetGameMap().Model.GetRelativeHeightmap(), GetGameMap().StaticModel.WallHeight));

            foreach (var user in _roomUserManager.GetUserList().ToList())
            {
                if (user == null)
                {
                    continue;
                }

                session.SendPacket(new UsersComposer(user));

                if (user.IsBot && user.BotData.DanceId > 0)
                {
                    session.SendPacket(new DanceComposer(user, user.BotData.DanceId));
                }
                else if (!user.IsBot && !user.IsPet && user.IsDancing)
                {
                    session.SendPacket(new DanceComposer(user, user.DanceId));
                }

                if (user.IsAsleep)
                {
                    session.SendPacket(new SleepComposer(user, true));
                }

                if (user.CarryItemId > 0 && user.CarryTimer > 0)
                {
                    session.SendPacket(new CarryObjectComposer(user.VirtualId, user.CarryItemId));
                }

                if (!user.IsBot && !user.IsPet && user.CurrentEffect > 0)
                {
                    session.SendPacket(new AvatarEffectComposer(user.VirtualId, user.CurrentEffect));
                }
            }

            session.SendPacket(new UserUpdateComposer(_roomUserManager.GetUserList().ToList()));
            session.SendPacket(new ObjectsComposer(GetRoomItemHandler().GetFloor.ToArray(), this));
            session.SendPacket(new ItemsComposer(GetRoomItemHandler().GetWall.ToArray(), this));
        }

        #region Tents
        public void AddTent(int TentId)
        {
            if (Tents.ContainsKey(TentId))
            {
                Tents.Remove(TentId);
            }

            Tents.Add(TentId, new List<RoomUser>());
        }

        public void RemoveTent(int TentId)
        {
            if (!Tents.ContainsKey(TentId))
            {
                return;
            }

            var Users = Tents[TentId];
            foreach (var User in Users.ToList())
            {
                if (User == null || User.GetClient() == null || User.GetClient().GetHabbo() == null)
                {
                    continue;
                }

                User.GetClient().GetHabbo().TentId = 0;
            }

            if (Tents.ContainsKey(TentId))
            {
                Tents.Remove(TentId);
            }
        }

        public void AddUserToTent(int TentId, RoomUser User)
        {
            if (User != null && User.GetClient() != null && User.GetClient().GetHabbo() != null)
            {
                if (!Tents.ContainsKey(TentId))
                {
                    Tents.Add(TentId, new List<RoomUser>());
                }

                if (!Tents[TentId].Contains(User))
                {
                    Tents[TentId].Add(User);
                }

                User.GetClient().GetHabbo().TentId = TentId;
            }
        }

        public void RemoveUserFromTent(int TentId, RoomUser User)
        {
            if (User != null && User.GetClient() != null && User.GetClient().GetHabbo() != null)
            {
                if (!Tents.ContainsKey(TentId))
                {
                    Tents.Add(TentId, new List<RoomUser>());
                }

                if (Tents[TentId].Contains(User))
                {
                    Tents[TentId].Remove(User);
                }

                User.GetClient().GetHabbo().TentId = 0;
            }
        }

        public void SendToTent(int Id, int TentId, IServerPacket Packet)
        {
            if (!Tents.ContainsKey(TentId))
            {
                return;
            }

            foreach (var User in Tents[TentId].ToList())
            {
                if (User == null || User.GetClient() == null || User.GetClient().GetHabbo() == null || User.GetClient().GetHabbo().GetIgnores().IgnoredUserIds().Contains(Id) || User.GetClient().GetHabbo().TentId != TentId)
                {
                    continue;
                }

                User.GetClient().SendPacket(Packet);
            }
        }
        #endregion

        #region Communication (Packets)
        public void SendPacket(IServerPacket packet, bool withRightsOnly = false)
        {
            if (packet == null)
            {
                return;
            }

            try
            {

                var users = _roomUserManager.GetUserList().ToList();

                if (_roomUserManager == null || users == null)
                {
                    return;
                }

                foreach (var user in users)
                {
                    if (user == null || user.IsBot)
                    {
                        continue;
                    }

                    if (user.GetClient() == null || user.GetClient().GetConnection() == null)
                    {
                        continue;
                    }

                    if (withRightsOnly && !CheckRights(user.GetClient()))
                    {
                        continue;
                    }

                    user.GetClient().SendPacket(packet);
                }
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }
        }

        public void BroadcastPacket(byte[] packet)
        {
            foreach (var user in _roomUserManager.GetUserList().ToList())
            {
                if (user == null || user.IsBot)
                {
                    continue;
                }

                if (user.GetClient() == null || user.GetClient().GetConnection() == null)
                {
                    continue;
                }

                user.GetClient().GetConnection().SendData(packet);
            }
        }

        public void SendPacket(List<ServerPacket> packets)
        {
            if (packets.Count == 0)
            {
                return;
            }

            try
            {
                var TotalBytes = new byte[0];
                var Current = 0;

                foreach (var packet in packets.ToList())
                {
                    var ToAdd = packet.GetBytes();
                    var NewLen = TotalBytes.Length + ToAdd.Length;

                    Array.Resize(ref TotalBytes, NewLen);

                    for (var i = 0; i < ToAdd.Length; i++)
                    {
                        TotalBytes[Current] = ToAdd[i];
                        Current++;
                    }
                }

                BroadcastPacket(TotalBytes);
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }
        }
        #endregion

        public void Dispose()
        {
            SendPacket(new CloseConnectionComposer());

            if (!mDisposed)
            {
                isCrashed = false;
                mDisposed = true;

                /* TODO: Needs reviewing */
                try
                {
                    if (ProcessTask != null && ProcessTask.IsCompleted)
                    {
                        ProcessTask.Dispose();
                    }
                }
                catch { }

                if (ActiveTrades.Count > 0)
                {
                    ActiveTrades.Clear();
                }

                TonerData = null;
                MoodlightData = null;

                if (MutedUsers.Count > 0)
                {
                    MutedUsers.Clear();
                }

                if (Tents.Count > 0)
                {
                    Tents.Clear();
                }

                if (UsersWithRights.Count > 0)
                {
                    UsersWithRights.Clear();
                }

                if (_gameManager != null)
                {
                    _gameManager.Dispose();
                    _gameManager = null;
                }

                if (_freeze != null)
                {
                    _freeze.Dispose();
                    _freeze = null;
                }

                if (_soccer != null)
                {
                    _soccer.Dispose();
                    _soccer = null;
                }

                if (_banzai != null)
                {
                    _banzai.Dispose();
                    _banzai = null;
                }

                if (_gamemap != null)
                {
                    _gamemap.Dispose();
                    _gamemap = null;
                }

                if (_gameItemHandler != null)
                {
                    _gameItemHandler.Dispose();
                    _gameItemHandler = null;
                }

                // Room Data?

                if (teambanzai != null)
                {
                    teambanzai.Dispose();
                    teambanzai = null;
                }

                if (teamfreeze != null)
                {
                    teamfreeze.Dispose();
                    teamfreeze = null;
                }

                if (_roomUserManager != null)
                {
                    _roomUserManager.Dispose();
                    _roomUserManager = null;
                }

                if (_roomItemHandling != null)
                {
                    _roomItemHandling.Dispose();
                    _roomItemHandling = null;
                }

                if (WordFilterList.Count > 0)
                {
                    WordFilterList.Clear();
                }

                if (_filterComponent != null)
                {
                    _filterComponent.Cleanup();
                }

                if (_wiredComponent != null)
                {
                    _wiredComponent.Cleanup();
                }

                if (_bansComponent != null)
                {
                    _bansComponent.Cleanup();
                }

                if (_tradingComponent != null)
                {
                    _tradingComponent.Cleanup();
                }
            }
        }
    }
}