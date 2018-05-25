namespace Plus.Game.Rooms.Chat.Commands.Moderator.Fun
{
    using Players;

    internal class UnFreezeCommand : IChatCommand
    {
        public string PermissionRequired => "command_unfreeze";

        public string Parameters => "%username%";

        public string Description => "Allow another user to walk again.";

        public void Execute(Player session, Room room, string[] Params)
        {
            if (Params.Length == 1)
            {
                session.SendWhisper("Please enter the username of the user you wish to un-freeze.");
                return;
            }

            var targetClient = Program.GameContext.PlayerController.GetClientByUsername(Params[1]);
            if (targetClient == null)
            {
                session.SendWhisper("An error occoured whilst finding that user, maybe they're not online.");
                return;
            }

            var targetUser = session.GetHabbo().CurrentRoom.GetRoomUserManager().GetRoomUserByHabbo(Params[1]);
            if (targetUser != null)
            {
                targetUser.Frozen = false;
            }

            session.SendWhisper("Successfully unfroze " + targetClient.GetHabbo().Username + "!");
        }
    }
}
