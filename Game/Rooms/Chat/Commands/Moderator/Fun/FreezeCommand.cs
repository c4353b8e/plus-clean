namespace Plus.Game.Rooms.Chat.Commands.Moderator.Fun
{
    using Players;

    internal class FreezeCommand : IChatCommand
    {
        public string PermissionRequired => "command_freeze";

        public string Parameters => "%username%";

        public string Description => "Prevent another user from walking.";

        public void Execute(Player session, Room room, string[] Params)
        {
            if (Params.Length == 1)
            {
                session.SendWhisper("Please enter the username of the user you wish to freeze.");
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
                targetUser.Frozen = true;
            }

            session.SendWhisper("Successfully froze " + targetClient.GetHabbo().Username + "!");
        }
    }
}
