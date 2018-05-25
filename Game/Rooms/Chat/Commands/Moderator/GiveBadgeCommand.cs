namespace Plus.Game.Rooms.Chat.Commands.Moderator
{
    using Players;

    internal class GiveBadgeCommand : IChatCommand
    {
        public string PermissionRequired => "command_give_badge";

        public string Parameters => "%username% %badge%";

        public string Description => "Give a badge to another user.";

        public void Execute(Player Session, Room Room, string[] Params)
        {
            if (Params.Length != 3)
            {
                Session.SendWhisper("Please enter a username and the code of the badge you'd like to give!");
                return;
            }

            var TargetClient = Program.GameContext.PlayerController.GetClientByUsername(Params[1]);
            if (TargetClient != null)
            {
                if (!TargetClient.GetHabbo().GetBadgeComponent().HasBadge(Params[2]))
                {
                    TargetClient.GetHabbo().GetBadgeComponent().GiveBadge(Params[2], true, TargetClient);
                    if (TargetClient.GetHabbo().Id != Session.GetHabbo().Id)
                    {
                        TargetClient.SendNotification("You have just been given a badge!");
                    }
                    else
                    {
                        Session.SendWhisper("You have successfully given yourself the badge " + Params[2] + "!");
                    }
                }
                else
                {
                    Session.SendWhisper("Oops, that user already has this badge (" + Params[2] + ") !");
                }
            }
            else
            {
                Session.SendWhisper("Oops, we couldn't find that target user!");
            }
        }
    }
}
