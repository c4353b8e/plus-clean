namespace Plus.Game.Users
{
    using System;
    using System.Collections;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Data;
    using Achievements;
    using Badges;
    using Clothing;
    using Communication.Packets.Outgoing.Handshake;
    using Communication.Packets.Outgoing.Inventory.Purse;
    using Communication.Packets.Outgoing.Navigator;
    using Communication.Packets.Outgoing.Rooms.Engine;
    using Communication.Packets.Outgoing.Rooms.Session;
    using Core.Logging;
    using Effects;
    using Groups;
    using Ignores;
    using Inventory;
    using Messenger;
    using Messenger.FriendBar;
    using Navigator.SavedSearches;
    using Permissions;
    using Players;
    using Process;
    using Relationships;
    using Rooms;
    using Rooms.Chat.Commands;
    using Subscriptions;
    using Utilities;

    public class Habbo
    {
        private static readonly ILogger Logger = new Logger<Habbo>();

        //Generic player values.

        //Abilitys triggered by generic events.

        //Player saving.
        private bool _disconnected;
        private bool _habboSaved;

        //Counters

        //Room related

        //Advertising reporting system.

        //Values generated within the game.

        //Fastfood

        //Just random fun stuff.

        //Anti-script placeholders.

        public List<int> RatedRooms;

        private Player _client;
        private readonly HabboStats _habboStats;
        private HabboMessenger Messenger;
        private ProcessComponent _process;
        public ArrayList FavoriteRooms;
        public Dictionary<int, int> quests;
        private BadgeComponent BadgeComponent;
        private InventoryComponent InventoryComponent;
        public Dictionary<int, Relationship> Relationships;
        public ConcurrentDictionary<string, UserAchievement> Achievements;

        private readonly DateTime _timeCached;

        private SearchesComponent _navigatorSearches;
        private EffectsComponent _fx;
        private ClothingComponent _clothing;
        private PermissionComponent _permissions;
        private IgnoresComponent _ignores;

        public Habbo(int Id, string Username, int Rank, string Motto, string Look, string Gender, int Credits, int ActivityPoints, int HomeRoom,
            bool HasFriendRequestsDisabled, int LastOnline, bool AppearOffline, bool HideInRoom, double CreateDate, int Diamonds,
            string machineID, string clientVolume, bool ChatPreference, bool FocusPreference, bool PetsMuted, bool BotsMuted, bool AdvertisingReportBlocked, double LastNameChange,
            int GOTWPoints, bool IgnoreInvites, double TimeMuted, double TradingLock, bool AllowGifts, int FriendBarState, bool DisableForcedEffects, bool AllowMimic, int VIPRank)
        {
            this.Id = Id;
            this.Username = Username;
            this.Rank = Rank;
            this.Motto = Motto;
            this.Look = Look;
            this.Gender = Gender.ToLower();
            FootballLook = HabboUtilities.FilterFigure(Look.ToLower());
            FootballGender = Gender.ToLower();
            this.Credits = Credits;
            Duckets = ActivityPoints;
            this.Diamonds = Diamonds;
            this.GOTWPoints = GOTWPoints;
            this.HomeRoom = HomeRoom;
            this.LastOnline = LastOnline;
            AccountCreated = CreateDate;
            ClientVolume = new List<int>();
            foreach (var Str in clientVolume.Split(','))
            {
                var Val = 0;
                if (int.TryParse(Str, out Val))
                {
                    ClientVolume.Add(int.Parse(Str));
                }
                else
                {
                    ClientVolume.Add(100);
                }
            }

            this.LastNameChange = LastNameChange;
            MachineId = machineID;
            this.ChatPreference = ChatPreference;
            this.FocusPreference = FocusPreference;
            IsExpert = IsExpert;

            this.AppearOffline = AppearOffline;
            AllowTradingRequests = true;//TODO
            AllowUserFollowing = true;//TODO
            AllowFriendRequests = HasFriendRequestsDisabled;//TODO
            AllowMessengerInvites = IgnoreInvites;
            AllowPetSpeech = PetsMuted;
            AllowBotSpeech = BotsMuted;
            AllowPublicRoomStatus = HideInRoom;
            AllowConsoleMessages = true;
            this.AllowGifts = AllowGifts;
            this.AllowMimic = AllowMimic;
            ReceiveWhispers = true;
            IgnorePublicWhispers = false;
            PlayingFastFood = false;
            FriendbarState = FriendBarStateUtility.GetEnum(FriendBarState);
            ChristmasDay = ChristmasDay;
            WantsToRideHorse = 0;
            TimeAFK = 0;
            this.DisableForcedEffects = DisableForcedEffects;
            this.VIPRank = VIPRank;

            _disconnected = false;
            _habboSaved = false;
            ChangingName = false;

            FloodTime = 0;
            FriendCount = 0;
            this.TimeMuted = TimeMuted;
            _timeCached = DateTime.Now;

            TradingLockExpiry = TradingLock;
            if (TradingLockExpiry > 0 && UnixUtilities.GetNow() > TradingLockExpiry)
            {
                TradingLockExpiry = 0;
                using (var dbClient = Program.DatabaseManager.GetQueryReactor())
                {
                    dbClient.RunQuery("UPDATE `user_info` SET `trading_locked` = '0' WHERE `user_id` = '" + Id + "' LIMIT 1");
                }
            }

            BannedPhraseCount = 0;
            SessionStart = UnixUtilities.GetNow();
            MessengerSpamCount = 0;
            MessengerSpamTime = 0;
            CreditsUpdateTick = Convert.ToInt32(Program.SettingsManager.TryGetValue("user.currency_scheduler.tick"));

            TentId = 0;
            HopperId = 0;
            IsHopping = false;
            TeleporterId = 0;
            IsTeleporting = false;
            TeleportingRoomID = 0;
            RoomAuthOk = false;
            CurrentRoomId = 0;

            HasSpoken = false;
            LastAdvertiseReport = 0;
            AdvertisingReported = false;
            AdvertisingReportedBlocked = AdvertisingReportBlocked;

            WiredInteraction = false;
            QuestLastCompleted = 0;
            InventoryAlert = false;
            IgnoreBobbaFilter = false;
            WiredTeleporting = false;
            CustomBubbleId = 0;
            OnHelperDuty = false;
            FastfoodScore = 0;
            PetId = 0;
            TempInt = 0;

            LastGiftPurchaseTime = DateTime.Now;
            LastMottoUpdateTime = DateTime.Now;
            LastClothingUpdateTime = DateTime.Now;
            LastForumMessageUpdateTime = DateTime.Now;

            GiftPurchasingWarnings = 0;
            MottoUpdateWarnings = 0;
            ClothingUpdateWarnings = 0;

            SessionGiftBlocked = false;
            SessionMottoBlocked = false;
            SessionClothingBlocked = false;

            FavoriteRooms = new ArrayList();
            Achievements = new ConcurrentDictionary<string, UserAchievement>();
            Relationships = new Dictionary<int, Relationship>();
            RatedRooms = new List<int>();

            //TODO: Nope.
            InitPermissions();

            #region Stats
            DataRow StatRow = null;
            using (var dbClient = Program.DatabaseManager.GetQueryReactor())
            {
                dbClient.SetQuery("SELECT `id`,`roomvisits`,`onlinetime`,`respect`,`respectgiven`,`giftsgiven`,`giftsreceived`,`dailyrespectpoints`,`dailypetrespectpoints`,`achievementscore`,`quest_id`,`quest_progress`,`groupid`,`tickets_answered`,`respectstimestamp`,`forum_posts` FROM `user_stats` WHERE `id` = @user_id LIMIT 1");
                dbClient.AddParameter("user_id", Id);
                StatRow = dbClient.GetRow();

                if (StatRow == null)//No row, add it yo
                {
                    dbClient.RunQuery("INSERT INTO `user_stats` (`id`) VALUES ('" + Id + "')");
                    dbClient.SetQuery("SELECT `id`,`roomvisits`,`onlinetime`,`respect`,`respectgiven`,`giftsgiven`,`giftsreceived`,`dailyrespectpoints`,`dailypetrespectpoints`,`achievementscore`,`quest_id`,`quest_progress`,`groupid`,`tickets_answered`,`respectstimestamp`,`forum_posts` FROM `user_stats` WHERE `id` = @user_id LIMIT 1");
                    dbClient.AddParameter("user_id", Id);
                    StatRow = dbClient.GetRow();
                }

                try
                {
                    _habboStats = new HabboStats(Convert.ToInt32(StatRow["roomvisits"]), Convert.ToDouble(StatRow["onlineTime"]), Convert.ToInt32(StatRow["respect"]), Convert.ToInt32(StatRow["respectGiven"]), Convert.ToInt32(StatRow["giftsGiven"]),
                        Convert.ToInt32(StatRow["giftsReceived"]), Convert.ToInt32(StatRow["dailyRespectPoints"]), Convert.ToInt32(StatRow["dailyPetRespectPoints"]), Convert.ToInt32(StatRow["AchievementScore"]),
                        Convert.ToInt32(StatRow["quest_id"]), Convert.ToInt32(StatRow["quest_progress"]), Convert.ToInt32(StatRow["groupid"]), Convert.ToString(StatRow["respectsTimestamp"]), Convert.ToInt32(StatRow["forum_posts"]));

                    if (Convert.ToString(StatRow["respectsTimestamp"]) != DateTime.Today.ToString("MM/dd"))
                    {
                        _habboStats.RespectsTimestamp = DateTime.Today.ToString("MM/dd");
                        SubscriptionData SubData = null;

                        var DailyRespects = 10;

                        if (_permissions.HasRight("mod_tool"))
                        {
                            DailyRespects = 20;
                        }
                        else if (Program.GameContext.GetSubscriptionManager().TryGetSubscriptionData(VIPRank, out SubData))
                        {
                            DailyRespects = SubData.Respects;
                        }

                        _habboStats.DailyRespectPoints = DailyRespects;
                        _habboStats.DailyPetRespectPoints = DailyRespects;

                        dbClient.RunQuery("UPDATE `user_stats` SET `dailyRespectPoints` = '" + DailyRespects + "', `dailyPetRespectPoints` = '" + DailyRespects + "', `respectsTimestamp` = '" + DateTime.Today.ToString("MM/dd") + "' WHERE `id` = '" + Id + "' LIMIT 1");
                    }
                }
                catch (Exception e)
                {
                    Logger.Error(e);
                }
            }

            Group G = null;
            if (!Program.GameContext.GetGroupManager().TryGetGroup(_habboStats.FavouriteGroupId, out G))
            {
                _habboStats.FavouriteGroupId = 0;
            }
            #endregion
        }

        public int Id { get; set; }

        public string Username { get; set; }

        public int Rank { get; set; }

        public string Motto { get; set; }

        public string Look { get; set; }

        public string Gender { get; set; }

        public string FootballLook { get; set; }

        public string FootballGender { get; set; }

        public int Credits { get; set; }

        public int Duckets { get; set; }

        public int Diamonds { get; set; }

        public int GOTWPoints { get; set; }

        public int HomeRoom { get; set; }

        public double LastOnline { get; set; }

        public double AccountCreated { get; set; }

        public List<int> ClientVolume { get; set; }

        public double LastNameChange { get; set; }

        public string MachineId { get; set; }

        public bool ChatPreference { get; set; }

        public bool FocusPreference { get; set; }

        public bool IsExpert { get; set; }

        public bool AppearOffline { get; set; }

        public int VIPRank { get; set; }

        public int TempInt { get; set; }

        public bool AllowTradingRequests { get; set; }

        public bool AllowUserFollowing { get; set; }

        public bool AllowFriendRequests { get; set; }

        public bool AllowMessengerInvites { get; set; }

        public bool AllowPetSpeech { get; set; }

        public bool AllowBotSpeech { get; set; }

        public bool AllowPublicRoomStatus { get; set; }

        public bool AllowConsoleMessages { get; set; }

        public bool AllowGifts { get; set; }

        public bool AllowMimic { get; set; }

        public bool ReceiveWhispers { get; set; }

        public bool IgnorePublicWhispers { get; set; }

        public bool PlayingFastFood { get; set; }

        public FriendBarState FriendbarState { get; set; }

        public int ChristmasDay { get; set; }

        public int WantsToRideHorse { get; set; }

        public int TimeAFK { get; set; }

        public bool DisableForcedEffects { get; set; }

        public bool ChangingName { get; set; }

        public int FriendCount { get; set; }

        public double FloodTime { get; set; }

        public int BannedPhraseCount { get; set; }

        public bool RoomAuthOk { get; set; }

        public int CurrentRoomId { get; set; }

        public int QuestLastCompleted { get; set; }

        public int MessengerSpamCount { get; set; }

        public double MessengerSpamTime { get; set; }

        public double TimeMuted { get; set; }

        public double TradingLockExpiry { get; set; }

        public double SessionStart { get; set; }

        public int TentId { get; set; }

        public int HopperId { get; set; }

        public bool IsHopping { get; set; }

        public int TeleporterId { get; set; }

        public bool IsTeleporting { get; set; }

        public int TeleportingRoomID { get; set; }

        public bool HasSpoken { get; set; }

        public double LastAdvertiseReport { get; set; }

        public bool AdvertisingReported { get; set; }

        public bool AdvertisingReportedBlocked { get; set; }

        public bool WiredInteraction { get; set; }

        public bool InventoryAlert { get; set; }

        public bool IgnoreBobbaFilter { get; set; }

        public bool WiredTeleporting { get; set; }

        public int CustomBubbleId { get; set; }

        public bool OnHelperDuty { get; set; }

        public int FastfoodScore { get; set; }

        public int PetId { get; set; }

        public int CreditsUpdateTick { get; set; }

        public IChatCommand IChatCommand { get; set; }

        public DateTime LastGiftPurchaseTime { get; set; }

        public DateTime LastMottoUpdateTime { get; set; }

        public DateTime LastClothingUpdateTime { get; set; }

        public DateTime LastForumMessageUpdateTime { get; set; }

        public int GiftPurchasingWarnings { get; set; }

        public int MottoUpdateWarnings { get; set; }

        public int ClothingUpdateWarnings { get; set; }

        public bool SessionGiftBlocked { get; set; }

        public bool SessionMottoBlocked { get; set; }

        public bool SessionClothingBlocked { get; set; }

        public HabboStats GetStats()
        {
            return _habboStats;
        }

        public bool InRoom => CurrentRoomId >= 1 && CurrentRoom != null;

        public Room CurrentRoom
        {
            get
            {
                if (CurrentRoomId <= 0)
                {
                    return null;
                }

                Room _room = null;
                if (Program.GameContext.GetRoomManager().TryGetRoom(CurrentRoomId, out _room))
                {
                    return _room;
                }

                return null;
            }
        }

        public bool CacheExpired()
        {
            var Span = DateTime.Now - _timeCached;
            return Span.TotalMinutes >= 30;
        }

        public string GetQueryString
        {
            get
            {
                _habboSaved = true;
                return "UPDATE `users` SET `online` = '0', `last_online` = '" + UnixUtilities.GetNow() + "', `activity_points` = '" + Duckets + "', `credits` = '" + Credits + "', `vip_points` = '" + Diamonds + "', `home_room` = '" + HomeRoom + "', `gotw_points` = '" + GOTWPoints + "', `time_muted` = '" + TimeMuted + "',`friend_bar_state` = '" + FriendBarStateUtility.GetInt(FriendbarState) + "' WHERE id = '" + Id + "' LIMIT 1;UPDATE `user_stats` SET `roomvisits` = '" + _habboStats.RoomVisits + "', `onlineTime` = '" + (UnixUtilities.GetNow() - SessionStart + _habboStats.OnlineTime) + "', `respect` = '" + _habboStats.Respect + "', `respectGiven` = '" + _habboStats.RespectGiven + "', `giftsGiven` = '" + _habboStats.GiftsGiven + "', `giftsReceived` = '" + _habboStats.GiftsReceived + "', `dailyRespectPoints` = '" + _habboStats.DailyRespectPoints + "', `dailyPetRespectPoints` = '" + _habboStats.DailyPetRespectPoints + "', `AchievementScore` = '" + _habboStats.AchievementPoints + "', `quest_id` = '" + _habboStats.QuestId + "', `quest_progress` = '" + _habboStats.QuestProgress + "', `groupid` = '" + _habboStats.FavouriteGroupId + "',`forum_posts` = '" + _habboStats.ForumPosts + "' WHERE `id` = '" + Id + "' LIMIT 1;";
            }
        }

        public bool InitProcess()
        {
            _process = new ProcessComponent();

            return _process.Init(this);
        }

        public bool InitSearches()
        {
            _navigatorSearches = new SearchesComponent();

            return _navigatorSearches.Init(this);
        }

        public bool InitFX()
        {
            _fx = new EffectsComponent();

            return _fx.Init(this);
        }

        public bool InitClothing()
        {
            _clothing = new ClothingComponent();

            return _clothing.Init(this);
        }

        public bool InitIgnores()
        {
            _ignores = new IgnoresComponent();

            return _ignores.Init(this);
        }

        private bool InitPermissions()
        {
            _permissions = new PermissionComponent();

            return _permissions.Init(this);
        }

        public void InitInformation(UserData.UserData data)
        {
            BadgeComponent = new BadgeComponent(this   , data);
            Relationships = data.Relations;
        }

        public void Init(Player client, UserData.UserData data)
        {
            Achievements = data.achievements;

            FavoriteRooms = new ArrayList();
            foreach (var id in data.favouritedRooms)
            {
                FavoriteRooms.Add(id);
            }

            _client = client;
            BadgeComponent = new BadgeComponent(this, data);
            InventoryComponent = new InventoryComponent(Id, client);

            quests = data.quests;

            Messenger = new HabboMessenger(Id);
            Messenger.Init(data.friends, data.requests);
            FriendCount = Convert.ToInt32(data.friends.Count);
            _disconnected = false;
            Relationships = data.Relations;

            InitSearches();
            InitFX();
            InitClothing();
            InitIgnores();
        }


        public PermissionComponent GetPermissions()
        {
            return _permissions;
        }

        public IgnoresComponent GetIgnores()
        {
            return _ignores;
        }

        public void OnDisconnect()
        {
            if (_disconnected)
            {
                return;
            }

            try
            {
                if (_process != null)
                {
                    _process.Dispose();
                }
            }
            catch { }

            _disconnected = true;

            Program.GameContext.PlayerController.UnregisterClient(Id, Username);

            if (!_habboSaved)
            {
                _habboSaved = true;
                using (var dbClient = Program.DatabaseManager.GetQueryReactor())
                {
                    dbClient.RunQuery("UPDATE `users` SET `online` = '0', `last_online` = '" + UnixUtilities.GetNow() + "', `activity_points` = '" + Duckets + "', `credits` = '" + Credits + "', `vip_points` = '" + Diamonds + "', `home_room` = '" + HomeRoom + "', `gotw_points` = '" + GOTWPoints + "', `time_muted` = '" + TimeMuted + "',`friend_bar_state` = '" + FriendBarStateUtility.GetInt(FriendbarState) + "' WHERE id = '" + Id + "' LIMIT 1;UPDATE `user_stats` SET `roomvisits` = '" + _habboStats.RoomVisits + "', `onlineTime` = '" + (UnixUtilities.GetNow() - SessionStart + _habboStats.OnlineTime) + "', `respect` = '" + _habboStats.Respect + "', `respectGiven` = '" + _habboStats.RespectGiven + "', `giftsGiven` = '" + _habboStats.GiftsGiven + "', `giftsReceived` = '" + _habboStats.GiftsReceived + "', `dailyRespectPoints` = '" + _habboStats.DailyRespectPoints + "', `dailyPetRespectPoints` = '" + _habboStats.DailyPetRespectPoints + "', `AchievementScore` = '" + _habboStats.AchievementPoints + "', `quest_id` = '" + _habboStats.QuestId + "', `quest_progress` = '" + _habboStats.QuestProgress + "', `groupid` = '" + _habboStats.FavouriteGroupId + "',`forum_posts` = '" + _habboStats.ForumPosts +"' WHERE `id` = '" + Id + "' LIMIT 1;");

                    if (GetPermissions().HasRight("mod_tickets"))
                    {
                        dbClient.RunQuery("UPDATE `moderation_tickets` SET `status` = 'open', `moderator_id` = '0' WHERE `status` ='picked' AND `moderator_id` = '" + Id + "'");
                    }
                }
            }

            Logger.Trace(Username + " has left the server.");
            Dispose();

            _client = null;

        }

        public void Dispose()
        {
            if (InventoryComponent != null)
            {
                InventoryComponent.SetIdleState();
            }

            if (InRoom && CurrentRoom != null)
            {
                CurrentRoom.GetRoomUserManager().RemoveUserFromRoom(_client, false, false);
            }

            if (Messenger != null)
            {
                Messenger.AppearOffline = true;
                Messenger.Destroy();
            }

            if (_fx != null)
            {
                _fx.Dispose();
            }

            if (_clothing != null)
            {
                _clothing.Dispose();
            }

            if (_permissions != null)
            {
                _permissions.Dispose();
            }

            if (_ignores != null)
            {
                _permissions.Dispose();
            }
        }

        public void CheckCreditsTimer()
        {
            try
            {
                CreditsUpdateTick--;

                if (CreditsUpdateTick <= 0)
                {
                    var CreditUpdate = Convert.ToInt32(Program.SettingsManager.TryGetValue("user.currency_scheduler.credit_reward"));
                    var DucketUpdate = Convert.ToInt32(Program.SettingsManager.TryGetValue("user.currency_scheduler.ducket_reward"));

                    SubscriptionData SubData = null;
                    if (Program.GameContext.GetSubscriptionManager().TryGetSubscriptionData(VIPRank, out SubData))
                    {
                        CreditUpdate += SubData.Credits;
                        DucketUpdate += SubData.Duckets;
                    }

                    Credits += CreditUpdate;
                    Duckets += DucketUpdate;

                    _client.SendPacket(new CreditBalanceComposer(Credits));
                    _client.SendPacket(new HabboActivityPointNotificationComposer(Duckets, DucketUpdate));

                    CreditsUpdateTick = Convert.ToInt32(Program.SettingsManager.TryGetValue("user.currency_scheduler.tick"));
                }
            }
            catch { }
        }

        public Player GetClient()
        {
            if (_client != null)
            {
                return _client;
            }

            return Program.GameContext.PlayerController.GetClientByUserId(Id);
        }

        public HabboMessenger GetMessenger()
        {
            return Messenger;
        }

        public BadgeComponent GetBadgeComponent()
        {
            return BadgeComponent;
        }

        public InventoryComponent GetInventoryComponent()
        {
            return InventoryComponent;
        }

        public SearchesComponent GetNavigatorSearches()
        {
            return _navigatorSearches;
        }

        public EffectsComponent Effects()
        {
            return _fx;
        }

        public ClothingComponent GetClothing()
        {
            return _clothing;
        }

        public int GetQuestProgress(int p)
        {
            var progress = 0;
            quests.TryGetValue(p, out progress);
            return progress;
        }

        public UserAchievement GetAchievementData(string p)
        {
            UserAchievement achievement = null;
            Achievements.TryGetValue(p, out achievement);
            return achievement;
        }

        public void ChangeName(string Username)
        {
            LastNameChange = UnixUtilities.GetNow();
            this.Username = Username;

            SaveKey("username", Username);
            SaveKey("last_change", LastNameChange.ToString());
        }

        public void SaveKey(string Key, string Value)
        {
            using (var dbClient = Program.DatabaseManager.GetQueryReactor())
            {
                dbClient.SetQuery("UPDATE `users` SET " + Key + " = @value WHERE `id` = '" + Id + "' LIMIT 1;");
                dbClient.AddParameter("value", Value);
                dbClient.RunQuery();
            }
        }

        public void PrepareRoom(int id, string password)
        {
            if (GetClient() == null || GetClient().GetHabbo() == null)
            {
                return;
            }

            if (GetClient().GetHabbo().InRoom)
            {
                Room OldRoom = null;
                if (!Program.GameContext.GetRoomManager().TryGetRoom(GetClient().GetHabbo().CurrentRoomId, out OldRoom))
                {
                    return;
                }

                if (OldRoom.GetRoomUserManager() != null)
                {
                    OldRoom.GetRoomUserManager().RemoveUserFromRoom(GetClient(), false, false);
                }
            }

            if (GetClient().GetHabbo().IsTeleporting && GetClient().GetHabbo().TeleportingRoomID != id)
            {
                GetClient().SendPacket(new CloseConnectionComposer());
                return;
            }

            Room Room = null;
            if (!Program.GameContext.GetRoomManager().TryLoadRoom(id, out Room))
            {
                GetClient().SendPacket(new CloseConnectionComposer());
                return;
            }

            if (Room.isCrashed)
            {
                GetClient().SendNotification("This room has crashed! :(");
                GetClient().SendPacket(new CloseConnectionComposer());
                return;
            }

            GetClient().GetHabbo().CurrentRoomId = Room.RoomId;

            if (Room.GetRoomUserManager().userCount >= Room.UsersMax && !GetClient().GetHabbo().GetPermissions().HasRight("room_enter_full") && GetClient().GetHabbo().Id != Room.OwnerId)
            {
                GetClient().SendPacket(new CantConnectComposer(1));
                GetClient().SendPacket(new CloseConnectionComposer());
                return;
            }


            if (!GetPermissions().HasRight("room_ban_override") && Room.GetBans().IsBanned(Id))
            {
                RoomAuthOk = false;
                GetClient().GetHabbo().RoomAuthOk = false;
                GetClient().SendPacket(new CantConnectComposer(4));
                GetClient().SendPacket(new CloseConnectionComposer());
                return;
            }

            GetClient().SendPacket(new OpenConnectionComposer());
            if (!Room.CheckRights(GetClient(), true, true) && !GetClient().GetHabbo().IsTeleporting && !GetClient().GetHabbo().IsHopping)
            {
                if (Room.Access == RoomAccess.Doorbell && !GetClient().GetHabbo().GetPermissions().HasRight("room_enter_locked"))
                {
                    if (Room.UserCount > 0)
                    {
                        GetClient().SendPacket(new DoorbellComposer(""));
                        Room.SendPacket(new DoorbellComposer(GetClient().GetHabbo().Username), true);
                        return;
                    }

                    GetClient().SendPacket(new FlatAccessDeniedComposer(""));
                    GetClient().SendPacket(new CloseConnectionComposer());
                    return;
                }

                if (Room.Access == RoomAccess.Password && !GetClient().GetHabbo().GetPermissions().HasRight("room_enter_locked"))
                {
                    if (password.ToLower() != Room.Password.ToLower() || string.IsNullOrWhiteSpace(password))
                    {
                        GetClient().SendPacket(new GenericErrorComposer(-100002));
                        GetClient().SendPacket(new CloseConnectionComposer());
                        return;
                    }
                }
            }

            if (!EnterRoom(Room))
            {
                GetClient().SendPacket(new CloseConnectionComposer());
            }
        }

        public bool EnterRoom(Room Room)
        {
            if (Room == null)
            {
                GetClient().SendPacket(new CloseConnectionComposer());
            }

            GetClient().SendPacket(new RoomReadyComposer(Room.RoomId, Room.ModelName));
            if (Room.Wallpaper != "0.0")
            {
                GetClient().SendPacket(new RoomPropertyComposer("wallpaper", Room.Wallpaper));
            }

            if (Room.Floor != "0.0")
            {
                GetClient().SendPacket(new RoomPropertyComposer("floor", Room.Floor));
            }

            GetClient().SendPacket(new RoomPropertyComposer("landscape", Room.Landscape));
            GetClient().SendPacket(new RoomRatingComposer(Room.Score, !(GetClient().GetHabbo().RatedRooms.Contains(Room.RoomId) || Room.OwnerId == GetClient().GetHabbo().Id)));

            using (var dbClient = Program.DatabaseManager.GetQueryReactor())
            {
                dbClient.RunQuery("INSERT INTO user_roomvisits (user_id,room_id,entry_timestamp,exit_timestamp,hour,minute) VALUES ('" + GetClient().GetHabbo().Id + "','" + GetClient().GetHabbo().CurrentRoomId + "','" + UnixUtilities.GetNow() + "','0','" + DateTime.Now.Hour + "','" + DateTime.Now.Minute + "');");// +
            }


            if (Room.OwnerId != Id)
            {
                GetClient().GetHabbo().GetStats().RoomVisits += 1;
                Program.GameContext.GetAchievementManager().ProgressAchievement(GetClient(), "ACH_RoomEntry", 1);
            }
            return true;
        }
    }
}