namespace Plus.Game.Rooms.Chat.Commands.Moderator
{
    using Players;

    internal class AlertCommand : IChatCommand
    {
        public string PermissionRequired => "command_alert_user";

        public string Parameters => "%username% %Messages%";

        public string Description => "Alert a user with a message of your choice.";

        public void Execute(Player Session, Room Room, string[] Params)
        {
            if (Params.Length == 1)
            {
                Session.SendWhisper("Please enter the username of the user you wish to alert.");
                return;
            }

            var TargetClient = Program.GameContext.PlayerController.GetClientByUsername(Params[1]);
            if (TargetClient == null)
            {
                Session.SendWhisper("An error occoured whilst finding that user, maybe they're not online.");
                return;
            }

            if (TargetClient.GetHabbo() == null)
            {
                Session.SendWhisper("An error occoured whilst finding that user, maybe they're not online.");
                return;
            }

            if (TargetClient.GetHabbo().Username == Session.GetHabbo().Username)
            {
                Session.SendWhisper("Get a life.");
                return;
            }

            var Message = CommandManager.MergeParams(Params, 2);

            TargetClient.SendNotification(Session.GetHabbo().Username + " alerted you with the following message:\n\n" + Message);
            Session.SendWhisper("Alert successfully sent to " + TargetClient.GetHabbo().Username);

        }
    }
}
