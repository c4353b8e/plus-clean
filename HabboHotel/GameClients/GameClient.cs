namespace Plus.HabboHotel.GameClients
{
    using System;
    using System.Text;
    using Communication;
    using Communication.ConnectionManager;
    using Communication.Encryption.Crypto.Prng;
    using Communication.Interfaces;
    using Communication.Packets.Incoming;
    using Communication.Packets.Outgoing.BuildersClub;
    using Communication.Packets.Outgoing.Handshake;
    using Communication.Packets.Outgoing.Inventory.Achievements;
    using Communication.Packets.Outgoing.Inventory.AvatarEffects;
    using Communication.Packets.Outgoing.Moderation;
    using Communication.Packets.Outgoing.Navigator;
    using Communication.Packets.Outgoing.Notifications;
    using Communication.Packets.Outgoing.Rooms.Chat;
    using Communication.Packets.Outgoing.Sound;
    using Core.Logging;
    using Users;
    using Users.Messenger.FriendBar;
    using Users.UserData;

    public class GameClient
    {
        private static readonly ILogger Logger = new Logger<GameClient>();

        private Habbo _habbo;
        public string MachineId;
        private bool _disconnected;
        public ARC4 Rc4Client;
        private GamePacketParser _packetParser;
        private ConnectionInformation _connection;
        public int PingCount { get; set; }

        public GameClient(int clientId, ConnectionInformation connection)
        {
            ConnectionId = clientId;
            _connection = connection;
            _packetParser = new GamePacketParser(this);

            PingCount = 0;
        }

        private void SwitchParserRequest()
        {
            _packetParser.SetConnection(_connection);
            _packetParser.OnNewPacket += Parser_onNewPacket;
            var data = (_connection.Parser as InitialPacketParser).CurrentData;
            _connection.Parser.Dispose();
            _connection.Parser = _packetParser;
            _connection.Parser.HandlePacketData(data);
        }

        private void Parser_onNewPacket(ClientPacket message)
        {
            try
            {
                Program.GameContext.GetPacketManager().TryExecutePacket(this, message);
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }
        }

        private void PolicyRequest()
        {
            _connection.SendData(Encoding.Default.GetBytes("<?xml version=\"1.0\"?>\r\n" +
                   "<!DOCTYPE cross-domain-policy SYSTEM \"/xml/dtds/cross-domain-policy.dtd\">\r\n" +
                   "<cross-domain-policy>\r\n" +
                   "<allow-access-from domain=\"*\" to-ports=\"1-31111\" />\r\n" +
                   "</cross-domain-policy>\x0"));
        }


        public void StartConnection()
        {
            if (_connection == null)
            {
                return;
            }

            PingCount = 0;

            (_connection.Parser as InitialPacketParser).PolicyRequest += PolicyRequest;
            (_connection.Parser as InitialPacketParser).SwitchParserRequest += SwitchParserRequest;
            _connection.StartPacketProcessing();
        }

        public bool TryAuthenticate(string authTicket)
        {
            try
            {
                var userData = UserDataFactory.GetUserData(authTicket, out var errorCode);
                if (errorCode == 1 || errorCode == 2)
                {
                    Disconnect();
                    return false;
                }

                #region Ban Checking
                //Let's have a quick search for a ban before we successfully authenticate..
                if (!string.IsNullOrEmpty(MachineId))
                {
                    if (Program.GameContext.GetModerationManager().IsBanned(MachineId, out _))
                    {
                        if (Program.GameContext.GetModerationManager().MachineBanCheck(MachineId))
                        {
                            Disconnect();
                            return false;
                        }
                    }
                }

                if (userData.user != null)
                {
                    if (Program.GameContext.GetModerationManager().IsBanned(userData.user.Username, out _))
                    {
                        if (Program.GameContext.GetModerationManager().UsernameBanCheck(userData.user.Username))
                        {
                            Disconnect();
                            return false;
                        }
                    }
                }
                #endregion

                if (userData.user == null) //Possible NPE
                {
                    return false;
                }

                Program.GameContext.GetClientManager().RegisterClient(this, userData.UserId, userData.user.Username);
                _habbo = userData.user;
                if (_habbo != null)
                {
                    userData.user.Init(this, userData);

                    SendPacket(new AuthenticationOKComposer());
                    SendPacket(new AvatarEffectsComposer(_habbo.Effects().GetAllEffects));
                    SendPacket(new NavigatorSettingsComposer(_habbo.HomeRoom));
                    SendPacket(new FavouritesComposer(userData.user.FavoriteRooms));
                    SendPacket(new FigureSetIdsComposer(_habbo.GetClothing().GetClothingParts));
                    SendPacket(new UserRightsComposer(_habbo.Rank));
                    SendPacket(new AvailabilityStatusComposer());
                    SendPacket(new AchievementScoreComposer(_habbo.GetStats().AchievementPoints));
                    SendPacket(new BuildersClubMembershipComposer());
                    SendPacket(new CfhTopicsInitComposer(Program.GameContext.GetModerationManager().UserActionPresets));

                    SendPacket(new BadgeDefinitionsComposer(Program.GameContext.GetAchievementManager().Achievements));
                    SendPacket(new SoundSettingsComposer(_habbo.ClientVolume, _habbo.ChatPreference, _habbo.AllowMessengerInvites, _habbo.FocusPreference, FriendBarStateUtility.GetInt(_habbo.FriendbarState)));
                    //SendMessage(new TalentTrackLevelComposer());

                    if (GetHabbo().GetMessenger() != null)
                    {
                        GetHabbo().GetMessenger().OnStatusChanged(true);
                    }

                    if (!string.IsNullOrEmpty(MachineId))
                    {
                        if (_habbo.MachineId != MachineId)
                        {
                            using (var dbClient = Program.DatabaseManager.GetQueryReactor())
                            {
                                dbClient.SetQuery("UPDATE `users` SET `machine_id` = @MachineId WHERE `id` = @id LIMIT 1");
                                dbClient.AddParameter("MachineId", MachineId);
                                dbClient.AddParameter("id", _habbo.Id);
                                dbClient.RunQuery();
                            }
                        }

                        _habbo.MachineId = MachineId;
                    }

                    if (Program.GameContext.GetPermissionManager().TryGetGroup(_habbo.Rank, out var group))
                    {
                        if (!string.IsNullOrEmpty(group.Badge))
                        {
                            if (!_habbo.GetBadgeComponent().HasBadge(group.Badge))
                            {
                                _habbo.GetBadgeComponent().GiveBadge(group.Badge, true, this);
                            }
                        }
                    }

                    if (Program.GameContext.GetSubscriptionManager().TryGetSubscriptionData(_habbo.VIPRank, out var subData))
                    {
                        if (!string.IsNullOrEmpty(subData.Badge))
                        {
                            if (!_habbo.GetBadgeComponent().HasBadge(subData.Badge))
                            {
                                _habbo.GetBadgeComponent().GiveBadge(subData.Badge, true, this);
                            }
                        }
                    }

                    if (!Program.GameContext.GetCacheManager().ContainsUser(_habbo.Id))
                    {
                        Program.GameContext.GetCacheManager().GenerateUser(_habbo.Id);
                    }

                    _habbo.Look = Program.FigureManager.ProcessFigure(_habbo.Look, _habbo.Gender, _habbo.GetClothing().GetClothingParts, true);
                    _habbo.InitProcess();
          
                    if (userData.user.GetPermissions().HasRight("mod_tickets"))
                    {
                        SendPacket(new ModeratorInitComposer(
                          Program.GameContext.GetModerationManager().UserMessagePresets,
                          Program.GameContext.GetModerationManager().RoomMessagePresets,
                          Program.GameContext.GetModerationManager().GetTickets));
                    }

                    if (Program.SettingsManager.TryGetValue("user.login.message.enabled") == "1")
                    {
                        SendPacket(new MotdNotificationComposer(Program.LanguageManager.TryGetValue("user.login.message")));
                    }

                    Program.GameContext.GetRewardManager().CheckRewards(this);
                    return true;
                }
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }

            return false;
        }

        public void SendWhisper(string message, int colour = 0)
        {
            if (GetHabbo() == null || GetHabbo().CurrentRoom == null)
            {
                return;
            }

            var user = GetHabbo().CurrentRoom.GetRoomUserManager().GetRoomUserByHabbo(GetHabbo().Username);
            if (user == null)
            {
                return;
            }

            SendPacket(new WhisperComposer(user.VirtualId, message, 0, colour == 0 ? user.LastBubble : colour));
        }

        public void SendNotification(string message)
        {
            SendPacket(new BroadcastMessageAlertComposer(message));
        }

        public void SendPacket(IServerPacket message)
        {
            if (GetConnection() == null)
            {
                return;
            }

            GetConnection().SendData(message.GetBytes());
        }

        public int ConnectionId { get; }

        public ConnectionInformation GetConnection()
        {
            return _connection;
        }

        public Habbo GetHabbo()
        {
            return _habbo;
        }

        public void Disconnect()
        {
            try
            {
                if (GetHabbo() != null)
                {
                    using (var dbClient = Program.DatabaseManager.GetQueryReactor())
                    {
                        dbClient.RunQuery(GetHabbo().GetQueryString);
                    }

                    GetHabbo().OnDisconnect();
                }
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }

            if (!_disconnected)
            {
                if (_connection != null)
                {
                    _connection.Dispose();
                }

                _disconnected = true;
            }
        }

        public void Dispose()
        {
            if (GetHabbo() != null)
            {
                GetHabbo().OnDisconnect();
            }

            MachineId = string.Empty;
            _disconnected = true;
            _habbo = null;
            _connection = null;
            Rc4Client = null;
            _packetParser = null;
        }
    }
}