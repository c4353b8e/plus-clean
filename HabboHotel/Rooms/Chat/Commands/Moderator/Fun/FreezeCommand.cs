namespace Plus.HabboHotel.Rooms.Chat.Commands.Moderator.Fun
{
    using GameClients;

    internal class FreezeCommand : IChatCommand
    {
        public string PermissionRequired => "command_freeze";

        public string Parameters => "%username%";

        public string Description => "Prevent another user from walking.";

        public void Execute(GameClient Session, Room Room, string[] Params)
        {
            if (Params.Length == 1)
            {
                Session.SendWhisper("Please enter the username of the user you wish to freeze.");
                return;
            }

            var TargetClient = Program.GameContext.GetClientManager().GetClientByUsername(Params[1]);
            if (TargetClient == null)
            {
                Session.SendWhisper("An error occoured whilst finding that user, maybe they're not online.");
                return;
            }

            var TargetUser = Session.GetHabbo().CurrentRoom.GetRoomUserManager().GetRoomUserByHabbo(Params[1]);
            if (TargetUser != null)
            {
                TargetUser.Frozen = true;
            }

            Session.SendWhisper("Successfully froze " + TargetClient.GetHabbo().Username + "!");
        }
    }
}
