namespace Plus.Game.Rooms.Chat.Commands.User
{
    using Communication.Packets.Outgoing.Handshake;
    using Players;
    using Users;
    using Utilities;

    internal class FlagMeCommand : IChatCommand
    {
        public string PermissionRequired => "command_flagme";

        public string Parameters => "";

        public string Description => "Gives you the option to change your username.";

        public void Execute(Player Session, Room Room, string[] Params)
        {
            if (!CanChangeName(Session.GetHabbo()))
            {
                Session.SendWhisper("Sorry, it seems you currently do not have the option to change your username!");
                return;
            }

            Session.GetHabbo().ChangingName = true;
            Session.SendNotification("Please be aware that if your username is deemed as inappropriate, you will be banned without question.\r\rAlso note that Staff will NOT change your username again should you have an issue with what you have chosen.\r\rClose this window and click yourself to begin choosing a new username!");
            Session.SendPacket(new UserObjectComposer(Session.GetHabbo()));
        }

        private bool CanChangeName(Habbo Habbo)
        {
            if (Habbo.Rank == 1 && Habbo.VIPRank == 0 && Habbo.LastNameChange == 0)
            {
                return true;
            }

            if (Habbo.Rank == 1 && Habbo.VIPRank == 1 && (Habbo.LastNameChange == 0 || UnixUtilities.GetNow() + 604800 > Habbo.LastNameChange))
            {
                return true;
            }

            if (Habbo.Rank == 1 && Habbo.VIPRank == 2 && (Habbo.LastNameChange == 0 || UnixUtilities.GetNow() + 86400 > Habbo.LastNameChange))
            {
                return true;
            }

            if (Habbo.Rank == 1 && Habbo.VIPRank == 3)
            {
                return true;
            }

            if (Habbo.GetPermissions().HasRight("mod_tool"))
            {
                return true;
            }

            return false;
        }
    }
}
