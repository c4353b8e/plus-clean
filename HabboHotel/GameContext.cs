namespace Plus.HabboHotel
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Achievements;
    using Badges;
    using Bots;
    using Cache;
    using Catalog;
    using Communication.Encryption;
    using Communication.Encryption.Keys;
    using Communication.Packets;
    using Communication.Packets.Outgoing.Moderation;
    using GameClients;
    using Games;
    using Groups;
    using Items;
    using LandingView;
    using Moderation;
    using Navigator;
    using Permissions;
    using Quests;
    using Rewards;
    using Rooms;
    using Rooms.Chat;
    using Subscriptions;
    using Talents;

    public class GameContext : IDisposable
    {
        public DateTime lastEvent;

        private readonly PacketManager _packetManager;
        private readonly GameClientManager _clientManager;
        private readonly ModerationManager _moderationManager;
        private readonly ItemDataManager _itemDataManager;
        private readonly CatalogManager _catalogManager;
        private readonly NavigatorManager _navigatorManager;
        private readonly RoomManager _roomManager;
        private readonly ChatManager _chatManager;
        private readonly GroupManager _groupManager;
        private readonly QuestManager _questManager;
        private readonly AchievementManager _achievementManager;
        private readonly TalentTrackManager _talentTrackManager;
        private readonly HotelViewManager _hotelViewManager;
        private readonly GameDataManager _gameDataManager;
        private readonly BotManager _botManager;
        private readonly CacheManager _cacheManager;
        private readonly RewardManager _rewardManager;
        private readonly BadgeManager _badgeManager;
        private readonly PermissionManager _permissionManager;
        private readonly SubscriptionManager _subscriptionManager;

        private bool _cycleEnded;
        private bool _cycleActive;
        private readonly Task _gameCycle;
        private readonly int _cycleSleepTime = 25;

        public string GameRevision = "";

        public GameContext()
        {
            HabboEncryptionV2.Initialize(new RSAKeys());

            _packetManager = new PacketManager();
            _clientManager = new GameClientManager();
            _moderationManager = new ModerationManager();
            _itemDataManager = new ItemDataManager();
            _catalogManager = new CatalogManager(_itemDataManager);
            _navigatorManager = new NavigatorManager();
            _roomManager = new RoomManager();
            _chatManager = new ChatManager();
            _groupManager = new GroupManager();
            _questManager = new QuestManager();
            _achievementManager = new AchievementManager();
            _talentTrackManager = new TalentTrackManager();
            _hotelViewManager = new HotelViewManager();
            _gameDataManager = new GameDataManager();
            _botManager = new BotManager();
            _cacheManager = new CacheManager();
            _rewardManager = new RewardManager();
            _badgeManager = new BadgeManager();
            _permissionManager = new PermissionManager();
            _subscriptionManager = new SubscriptionManager();

            _gameCycle = new Task(GameCycle);
            _gameCycle.Start();

            _cycleActive = true;
        }

        private void GameCycle()
        {
            while (_cycleActive)
            {
                _cycleEnded = false;

                Program.GameContext.GetRoomManager().OnCycle();
                Program.GameContext.GetClientManager().OnCycle();

                _cycleEnded = true;
                Thread.Sleep(_cycleSleepTime);
            }
        }

        private void StopGameLoop()
        {
            _cycleActive = false;

            while (!_cycleEnded)
            {
                Thread.Sleep(_cycleSleepTime);
            }
        }

        public PacketManager GetPacketManager()
        {
            return _packetManager;
        }

        public GameClientManager GetClientManager()
        {
            return _clientManager;
        }

        public CatalogManager GetCatalog()
        {
            return _catalogManager;
        }

        public NavigatorManager GetNavigator()
        {
            return _navigatorManager;
        }

        public ItemDataManager GetItemManager()
        {
            return _itemDataManager;
        }

        public RoomManager GetRoomManager()
        {
            return _roomManager;
        }

        public AchievementManager GetAchievementManager()
        {
            return _achievementManager;
        }

        public TalentTrackManager GetTalentTrackManager()
        {
            return _talentTrackManager;
        }

        public ModerationManager GetModerationManager()
        {
            return _moderationManager;
        }

        public PermissionManager GetPermissionManager()
        {
            return _permissionManager;
        }

        public SubscriptionManager GetSubscriptionManager()
        {
            return _subscriptionManager;
        }

        public QuestManager GetQuestManager()
        {
            return _questManager;
        }

        public GroupManager GetGroupManager()
        {
            return _groupManager;
        }
        
        public HotelViewManager GetLandingManager()
        {
            return _hotelViewManager;
        }

        public ChatManager GetChatManager()
        {
            return _chatManager;
        }

        public GameDataManager GetGameDataManager()
        {
            return _gameDataManager;
        }

        public BotManager GetBotManager()
        {
            return _botManager;
        }

        public CacheManager GetCacheManager()
        {
            return _cacheManager;
        }

        public RewardManager GetRewardManager()
        {
            return _rewardManager;
        }

        public BadgeManager GetBadgeManager()
        {
            return _badgeManager;
        }

        public void Dispose()
        {
            GetClientManager().SendPacket(new BroadcastMessageAlertComposer(Program.LanguageManager.TryGetValue("server.shutdown.message")));
            StopGameLoop();

            GetPacketManager().UnregisterAll();
            GetPacketManager().WaitForAllToComplete();

            GetClientManager().CloseAll();

            GetRoomManager().Dispose();
        }
    }
}