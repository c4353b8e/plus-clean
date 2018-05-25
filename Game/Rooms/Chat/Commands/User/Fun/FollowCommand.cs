namespace Plus.Game.Rooms.Chat.Commands.User.Fun
{
    using Players;

    internal class FollowCommand : IChatCommand
    {
        public string PermissionRequired => "command_follow";

        public string Parameters => "%username%";

        public string Description => "Want to visit a specific user? Use this command!";

        public void Execute(Player Session, Room Room, string[] Params)
        {
            if (Params.Length == 1)
            {
                Session.SendWhisper("Please enter the username of the user you wish to follow.");
                return;
            }

            var TargetClient = Program.GameContext.PlayerController.GetClientByUsername(Params[1]);
            if (TargetClient == null)
            {
                Session.SendWhisper("An error occoured whilst finding that user, maybe they're not online.");
                return;
            }

            if (TargetClient.GetHabbo().CurrentRoom == Session.GetHabbo().CurrentRoom)
            {
                Session.SendWhisper("Hey you, open your eyes! " + TargetClient.GetHabbo().Username + " is in this room!");
                return;
            }

            if (TargetClient.GetHabbo().Username == Session.GetHabbo().Username)
            {
                Session.SendWhisper("Sadooooooooo!");
                return;
            }

            if (!TargetClient.GetHabbo().InRoom)
            {
                Session.SendWhisper("That user currently isn't in a room!");
                return;
            }

            if (TargetClient.GetHabbo().CurrentRoom.Access != RoomAccess.Open && !Session.GetHabbo().GetPermissions().HasRight("mod_tool"))
            {
                Session.SendWhisper("Oops, the room that user is either locked, passworded or invisible. You cannot follow!");
                return;
            }

            Session.GetHabbo().PrepareRoom(TargetClient.GetHabbo().CurrentRoom.RoomId, "");
        }
    }
}
