namespace Plus.Game.Rooms.Chat.Commands
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Administrator;
    using Communication.Packets.Outgoing.Notifications;
    using Events;
    using Items.Wired;
    using Moderator;
    using Moderator.Fun;
    using Players;
    using User;
    using User.Fun;
    using Utilities;

    public class CommandManager
    {
        private readonly string _prefix;

        private readonly Dictionary<string, IChatCommand> _commands;

        public CommandManager(string Prefix)
        {
            _prefix = Prefix;
            _commands = new Dictionary<string, IChatCommand>();

            RegisterVIP();
            RegisterUser();
            RegisterEvents();
            RegisterModerator();
            RegisterAdministrator();
        }

        public bool Parse(Player Session, string Message)
        {
            if (Session?.GetHabbo() == null || Session.GetHabbo().CurrentRoom == null)
            {
                return false;
            }

            if (!Message.StartsWith(_prefix))
            {
                return false;
            }

            if (Message == _prefix + "commands")
            {
                var List = new StringBuilder();
                List.Append("This is the list of commands you have available:\n");
                foreach (var CmdList in _commands.ToList())
                {
                    if (!string.IsNullOrEmpty(CmdList.Value.PermissionRequired))
                    {
                        if (!Session.GetHabbo().GetPermissions().HasCommand(CmdList.Value.PermissionRequired))
                        {
                            continue;
                        }
                    }

                    List.Append(_prefix + CmdList.Key + " " + CmdList.Value.Parameters + " - " + CmdList.Value.Description + "\n");
                }
                Session.SendPacket(new MotdNotificationComposer(List.ToString()));
                return true;
            }

            Message = Message.Substring(1);
            var Split = Message.Split(' ');

            if (Split.Length == 0)
            {
                return false;
            }

            IChatCommand Cmd = null;
            if (_commands.TryGetValue(Split[0].ToLower(), out Cmd))
            {
                if (Session.GetHabbo().GetPermissions().HasRight("mod_tool"))
                {
                    LogCommand(Session.GetHabbo().Id, Message, Session.GetHabbo().MachineId);
                }

                if (!string.IsNullOrEmpty(Cmd.PermissionRequired))
                {
                    if (!Session.GetHabbo().GetPermissions().HasCommand(Cmd.PermissionRequired))
                    {
                        return false;
                    }
                }


                Session.GetHabbo().IChatCommand = Cmd;
                Session.GetHabbo().CurrentRoom.GetWired().TriggerEvent(WiredBoxType.TriggerUserSaysCommand, Session.GetHabbo(), this);

                Cmd.Execute(Session, Session.GetHabbo().CurrentRoom, Split);
                return true;
            }
            return false;
        }

        private void RegisterVIP()
        {
            Register("spull", new SuperPullCommand());
        }

        private void RegisterEvents()
        {
            Register("eha", new EventAlertCommand());
            Register("eventalert", new EventAlertCommand());
        }

        private void RegisterUser()
        {
            Register("about", new InfoCommand());
            Register("pickall", new PickAllCommand());
            Register("ejectall", new EjectAllCommand());
            Register("lay", new LayCommand());
            Register("sit", new SitCommand());
            Register("stand", new StandCommand());
            Register("mutepets", new MutePetsCommand());
            Register("mutebots", new MuteBotsCommand());

            Register("mimic", new MimicCommand());
            Register("dance", new DanceCommand());
            Register("push", new PushCommand());
            Register("pull", new PullCommand());
            Register("enable", new EnableCommand());
            Register("follow", new FollowCommand());
            Register("faceless", new FacelessCommand());
            Register("moonwalk", new MoonwalkCommand());

            Register("unload", new UnloadCommand());
            Register("regenmaps", new RegenMaps());
            Register("emptyitems", new EmptyItems());
            Register("setmax", new SetMaxCommand());
            Register("setspeed", new SetSpeedCommand());
            Register("disablediagonal", new DisableDiagonalCommand());
            Register("flagme", new FlagMeCommand());

            Register("stats", new StatsCommand());
            Register("kickpets", new KickPetsCommand());
            Register("kickbots", new KickBotsCommand());

            Register("room", new RoomCommand());
            Register("dnd", new DNDCommand());
            Register("disablegifts", new DisableGiftsCommand());
            Register("convertcredits", new ConvertCreditsCommand());
            Register("disablewhispers", new DisableWhispersCommand());
            Register("disablemimic", new DisableMimicCommand()); ;

            Register("pet", new PetCommand());
            Register("spush", new SuperPushCommand());
            Register("superpush", new SuperPushCommand());

        }

        private void RegisterModerator()
        {
            Register("ban", new BanCommand());
            Register("mip", new MIPCommand());
            Register("ipban", new IPBanCommand());

            Register("ui", new UserInfoCommand());
            Register("userinfo", new UserInfoCommand());
            Register("sa", new StaffAlertCommand());
            Register("roomunmute", new RoomUnmuteCommand());
            Register("roommute", new RoomMuteCommand());
            Register("roombadge", new RoomBadgeCommand());
            Register("roomalert", new RoomAlertCommand());
            Register("roomkick", new RoomKickCommand());
            Register("mute", new MuteCommand());
            Register("smute", new MuteCommand());
            Register("unmute", new UnmuteCommand());
            Register("massbadge", new MassBadgeCommand());
            Register("kick", new KickCommand());
            Register("skick", new KickCommand());
            Register("ha", new HotelAlertCommand());
            Register("hotelalert", new HotelAlertCommand());
            Register("hal", new HALCommand());
            Register("give", new GiveCommand());
            Register("givebadge", new GiveBadgeCommand());
            Register("dc", new DisconnectCommand());
            Register("kill", new DisconnectCommand());
            Register("Disconnect", new DisconnectCommand());
            Register("alert", new AlertCommand());
            Register("tradeban", new TradeBanCommand());

            Register("teleport", new TeleportCommand());
            Register("summon", new SummonCommand());
            Register("override", new OverrideCommand());
            Register("massenable", new MassEnableCommand());
            Register("massdance", new MassDanceCommand());
            Register("freeze", new FreezeCommand());
            Register("unfreeze", new UnFreezeCommand());
            Register("fastwalk", new FastwalkCommand());
            Register("superfastwalk", new SuperFastwalkCommand());
            Register("coords", new CoordsCommand());
            Register("alleyesonme", new AllEyesOnMeCommand());
            Register("allaroundme", new AllAroundMeCommand());
            Register("forcesit", new ForceSitCommand());

            Register("ignorewhispers", new IgnoreWhispersCommand());
            Register("forced_effects", new DisableForcedFXCommand());

            Register("makesay", new MakeSayCommand());
            Register("flaguser", new FlagUserCommand());
        }

        private void RegisterAdministrator()
        {
            Register("bubble", new BubbleCommand());
            Register("update", new UpdateCommand());
            Register("deletegroup", new DeleteGroupCommand());
            Register("carry", new CarryCommand());
            Register("goto", new GOTOCommand());
        }

        public void Register(string CommandText, IChatCommand Command)
        {
            _commands.Add(CommandText, Command);
        }

        public static string MergeParams(string[] Params, int Start)
        {
            var Merged = new StringBuilder();
            for (var i = Start; i < Params.Length; i++)
            {
                if (i > Start)
                {
                    Merged.Append(" ");
                }

                Merged.Append(Params[i]);
            }

            return Merged.ToString();
        }

        public void LogCommand(int UserId, string Data, string MachineId)
        {
            using (var dbClient = Program.DatabaseManager.GetQueryReactor())
            {
                dbClient.SetQuery("INSERT INTO `logs_client_staff` (`user_id`,`data_string`,`machine_id`, `timestamp`) VALUES (@UserId,@Data,@MachineId,@Timestamp)");
                dbClient.AddParameter("UserId", UserId);
                dbClient.AddParameter("Data", Data);
                dbClient.AddParameter("MachineId", MachineId);
                dbClient.AddParameter("Timestamp", UnixUtilities.GetNow());
                dbClient.RunQuery();
            }
        }

        public bool TryGetCommand(string Command, out IChatCommand IChatCommand)
        {
            return _commands.TryGetValue(Command, out IChatCommand);
        }
    }
}
