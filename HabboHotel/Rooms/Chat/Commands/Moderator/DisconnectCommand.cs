namespace Plus.HabboHotel.Rooms.Chat.Commands.Moderator
{
    using GameClients;

    internal class DisconnectCommand :IChatCommand
    {
        public string PermissionRequired => "command_disconnect";

        public string Parameters => "%username%";

        public string Description => "Disconnects another user from the hotel.";

        public void Execute(GameClient Session, Room Room, string[] Params)
        {
            if (Params.Length == 1)
            {
                Session.SendWhisper("Please enter the username of the user you wish to Disconnect.");
                return;
            }

            var TargetClient = Program.GameContext.GetClientManager().GetClientByUsername(Params[1]);
            if (TargetClient == null)
            {
                Session.SendWhisper("An error occoured whilst finding that user, maybe they're not online.");
                return;
            }

            if (TargetClient.GetHabbo().GetPermissions().HasRight("mod_tool") && !Session.GetHabbo().GetPermissions().HasRight("mod_disconnect_any"))
            {
                Session.SendWhisper("You are not allowed to Disconnect that user.");
                return;
            }

            TargetClient.GetConnection().Dispose();
        }
    }
}
